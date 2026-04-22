namespace BusBooking.Api.Application.DTOs;

public record ProcessPaymentRequest(Guid BookingId, Guid TransactionId, bool IsSuccess, string CardNumber);
public record PaymentResponse(Guid PaymentId, Guid TransactionId, string PaymentStatus);
