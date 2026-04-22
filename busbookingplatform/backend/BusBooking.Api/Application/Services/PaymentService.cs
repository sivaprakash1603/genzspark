using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext db, IEmailService emailService, ILogger<PaymentService> logger)
    {
        _db = db;
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

        if (request.IsSuccess)
        {
            booking.BookingStatus = "Confirmed";

            _logger.LogInformation("Payment success. Booking confirmed. BookingId={BookingId} Amount={Amount}", booking.Id, booking.TotalAmount);

            await _emailService.SendAsync(passenger.Email, "Payment Successful", "Payment successful and booking confirmed.", "payment-success", passengerId);
            await _emailService.SendAsync(passenger.Email, "Booking Confirmed", "Your booking is confirmed.", "booking-confirmed", passengerId);
        }
        else
        {
            booking.BookingStatus = "Failed";

            _logger.LogWarning("Payment failed. Booking marked failed. BookingId={BookingId} Amount={Amount}", booking.Id, booking.TotalAmount);

            await _emailService.SendAsync(passenger.Email, "Payment Failed", "Payment failed for your booking.", "payment-failed", passengerId);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Payment persisted. PaymentId={PaymentId} BookingId={BookingId} Status={Status}", payment.Id, booking.Id, payment.PaymentStatus);

        return new PaymentResponse(payment.Id, payment.TransactionId, payment.PaymentStatus);
    }
}
