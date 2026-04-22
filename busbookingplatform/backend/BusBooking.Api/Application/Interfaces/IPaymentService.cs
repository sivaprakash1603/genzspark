using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessAsync(Guid passengerId, ProcessPaymentRequest request);
}
