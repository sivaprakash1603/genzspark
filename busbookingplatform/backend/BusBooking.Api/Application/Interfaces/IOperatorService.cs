using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IOperatorService
{
    Task AddBusAsync(Guid operatorUserId, AddBusRequest request);
    Task DisableBusTemporarilyAsync(Guid operatorUserId, Guid busId);
    Task EnableBusTemporarilyAsync(Guid operatorUserId, Guid busId);
    Task RemoveBusAsync(Guid operatorUserId, Guid busId);
    Task<List<OperatorBookingResponse>> GetBookingsAsync(Guid operatorUserId);
    Task<OperatorRevenueResponse> GetRevenueAsync(Guid operatorUserId);
}
