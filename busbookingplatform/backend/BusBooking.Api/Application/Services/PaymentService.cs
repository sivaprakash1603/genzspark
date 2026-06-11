using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using BusBooking.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

internal class PaymentService : IPaymentService
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

        var booking = await GetBookingAsync(passengerId, request.BookingId);
        var payment = RecordPayment(request, booking.TotalAmount);
        var passenger = await _db.Users.FirstAsync(x => x.Id == passengerId);

        string? ticketNumber = null;

        if (request.IsSuccess)
        {
            ticketNumber = await HandleSuccessfulPaymentAsync(passengerId, booking);
        }
        else
        {
            HandleFailedPayment(booking);
        }

        await _db.SaveChangesAsync();
        await SendPaymentEmailAsync(passengerId, passenger.Email, request.IsSuccess, booking.Id, ticketNumber);

        _logger.LogInformation("Payment persisted. PaymentId={PaymentId} BookingId={BookingId} Status={Status}", payment.Id, booking.Id, payment.PaymentStatus.ToString());
        return new PaymentResponse(payment.Id, payment.TransactionId, payment.PaymentStatus.ToString());
    }

    private async Task<Domain.Entities.Booking> GetBookingAsync(Guid passengerId, Guid bookingId)
    {
        return await _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId && x.PassengerId == passengerId) ?? throw new InvalidOperationException("Booking not found");
    }

    private Domain.Entities.Payment RecordPayment(ProcessPaymentRequest request, decimal amount)
    {
        var payment = new Domain.Entities.Payment
        {
            BookingId = request.BookingId, TransactionId = request.TransactionId,
            PaymentStatus = request.IsSuccess ? PaymentStatus.Success : PaymentStatus.Failed,
            Amount = amount, CardLast4 = request.CardNumber.Length >= 4 ? request.CardNumber[^4..] : request.CardNumber,
            ResponsePayload = $"{{\"isSuccess\":{request.IsSuccess.ToString().ToLowerInvariant()}}}"
        };
        _db.Payments.Add(payment);
        return payment;
    }

    private async Task<string> HandleSuccessfulPaymentAsync(Guid passengerId, Domain.Entities.Booking booking)
    {
        booking.BookingStatus = BookingStatus.Confirmed;
        var ticketNumber = await GenerateTicketAsync(booking.Id);
        _logger.LogInformation("Payment success. Booking confirmed. BookingId={BookingId} Amount={Amount}", booking.Id, booking.TotalAmount);
        
        await UnlockSeatsAsync(passengerId, booking.Id);
        return ticketNumber;
    }

    private void HandleFailedPayment(Domain.Entities.Booking booking)
    {
        booking.BookingStatus = BookingStatus.PendingPayment;
        _logger.LogWarning("Payment failed. Booking remains pending payment. BookingId={BookingId} Amount={Amount}", booking.Id, booking.TotalAmount);
    }

    private async Task<string> GenerateTicketAsync(Guid bookingId)
    {
        var existingTicket = await _db.Tickets.FirstOrDefaultAsync(x => x.BookingId == bookingId);
        if (existingTicket != null) return existingTicket.TicketNumber;

        var ticketNumber = $"TKT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        _db.Tickets.Add(new Domain.Entities.Ticket { BookingId = bookingId, TicketNumber = ticketNumber, IssuedAt = DateTime.UtcNow, DownloadUrl = $"/api/bookings/{bookingId}/ticket" });
        return ticketNumber;
    }

    private async Task UnlockSeatsAsync(Guid passengerId, Guid bookingId)
    {
        var bookingSeats = await _db.BookingPassengers.Include(x => x.Seat).Where(x => x.BookingId == bookingId).Select(x => x.Seat).ToListAsync();
        foreach (var seat in bookingSeats.Where(s => s != null))
        {
            if (seat!.LockedByUserId == passengerId)
            {
                seat.LockedUntil = null;
                seat.LockedByUserId = null;
            }
        }
    }

    private async Task SendPaymentEmailAsync(Guid passengerId, string email, bool isSuccess, Guid bookingId, string? ticketNumber)
    {
        if (!isSuccess)
        {
            await _emailService.SendAsync(email, "Payment Failed", "Payment failed for your booking.", "payment-failed", passengerId);
            return;
        }

        try
        {
            var ticketPdf = await _bookingService.GetTicketPdfAsync(passengerId, bookingId);
            await _emailService.SendAsync(email, "Booking Confirmed - Ticket Attached", $"Your booking is confirmed. Your ticket number is {ticketNumber}. Please find your e-ticket attached as a PDF.", "booking-confirmed", passengerId, ticketPdf, $"Ticket-{ticketNumber}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate/send ticket email with attachment for BookingId={BookingId}", bookingId);
            await _emailService.SendAsync(email, "Booking Confirmed", $"Your booking is confirmed. Your ticket number is {ticketNumber}. (PDF attachment failed, please download from dashboard)", "booking-confirmed", passengerId);
        }
    }
}
