using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IBookingService _bookingService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext db, IBookingService bookingService, IEmailService emailService, ILogger<PaymentService> logger)
    {
        _db = db;
        _bookingService = bookingService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<PaymentResponse> ProcessAsync(Guid passengerId, ProcessPaymentRequest request)
    {
        _logger.LogInformation("ProcessPayment requested. PassengerId={PassengerId} BookingId={BookingId} TransactionId={TransactionId} IsSuccess={IsSuccess}", passengerId, request.BookingId, request.TransactionId, request.IsSuccess);

        var booking = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == request.BookingId && x.PassengerId == passengerId)
            ?? throw new InvalidOperationException("Booking not found");

        var paymentStatus = request.IsSuccess ? "Success" : "Failed";

        var payment = new Domain.Entities.Payment
        {
            BookingId = request.BookingId,
            TransactionId = request.TransactionId,
            PaymentStatus = paymentStatus,
            Amount = booking.TotalAmount,
            CardLast4 = request.CardNumber.Length >= 4 ? request.CardNumber[^4..] : request.CardNumber,
            ResponsePayload = $"{{\"isSuccess\":{request.IsSuccess.ToString().ToLowerInvariant()}}}"
        };

        _db.Payments.Add(payment);

        var passenger = await _db.Users.FirstAsync(x => x.Id == passengerId);
        string? ticketNumber = null;

        if (request.IsSuccess)
        {
            booking.BookingStatus = "Confirmed";

            var existingTicket = await _db.Tickets.FirstOrDefaultAsync(x => x.BookingId == booking.Id);
            ticketNumber = existingTicket?.TicketNumber;
            if (existingTicket is null)
            {
                ticketNumber = $"TKT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
                _db.Tickets.Add(new Domain.Entities.Ticket
                {
                    BookingId = booking.Id,
                    TicketNumber = ticketNumber,
                    IssuedAt = DateTime.UtcNow,
                    DownloadUrl = $"/api/bookings/{booking.Id}/ticket"
                });
            }

            _logger.LogInformation("Payment success. Booking confirmed. BookingId={BookingId} Amount={Amount}", booking.Id, booking.TotalAmount);

            // Unlock seats upon successful booking
            var bookingSeats = await _db.BookingPassengers
                .Include(x => x.Seat)
                .Where(x => x.BookingId == booking.Id)
                .Select(x => x.Seat)
                .ToListAsync();

            foreach (var seat in bookingSeats.Where(s => s != null))
            {
                if (seat!.LockedByUserId == passengerId)
                {
                    seat.LockedUntil = null;
                    seat.LockedByUserId = null;
                }
            }
        }
        else
        {
            booking.BookingStatus = "PendingPayment";
            _logger.LogWarning("Payment failed. Booking remains pending payment. BookingId={BookingId} Amount={Amount}", booking.Id, booking.TotalAmount);
        }

        await _db.SaveChangesAsync();

        if (request.IsSuccess)
        {
            try
            {
                var ticketPdf = await _bookingService.GetTicketPdfAsync(passengerId, booking.Id);
                await _emailService.SendAsync(passenger.Email, "Booking Confirmed - Ticket Attached", 
                    $"Your booking is confirmed. Your ticket number is {ticketNumber}. Please find your e-ticket attached as a PDF.", 
                    "booking-confirmed", passengerId, ticketPdf, $"Ticket-{ticketNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate/send ticket email with attachment for BookingId={BookingId}", booking.Id);
                await _emailService.SendAsync(passenger.Email, "Booking Confirmed", 
                    $"Your booking is confirmed. Your ticket number is {ticketNumber}. (PDF attachment failed, please download from dashboard)", 
                    "booking-confirmed", passengerId);
            }
        }
        else
        {
            await _emailService.SendAsync(passenger.Email, "Payment Failed", "Payment failed for your booking.", "payment-failed", passengerId);
        }

        _logger.LogInformation("Payment persisted. PaymentId={PaymentId} BookingId={BookingId} Status={Status}", payment.Id, booking.Id, payment.PaymentStatus);
        return new PaymentResponse(payment.Id, payment.TransactionId, payment.PaymentStatus);
    }
}
