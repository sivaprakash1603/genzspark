using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IOperatorService
{
    Task AddVehicleAsync(Guid operatorUserId, AddVehicleRequest request);
    Task<List<VehicleResponse>> GetMyVehiclesAsync(Guid operatorUserId);
    Task AddBusAsync(Guid operatorUserId, AddBusRequest request);
    Task UpdateBusAsync(Guid operatorUserId, Guid busId, UpdateBusRequest request);
    Task<List<OperatorBusResponse>> GetMyBusesAsync(Guid operatorUserId);
    Task DisableBusTemporarilyAsync(Guid operatorUserId, Guid busId);
    Task EnableBusTemporarilyAsync(Guid operatorUserId, Guid busId);
    Task RemoveBusAsync(Guid operatorUserId, Guid busId);
    Task<List<OperatorBookingResponse>> GetBookingsAsync(Guid operatorUserId);
    Task<OperatorRevenueResponse> GetRevenueAsync(Guid operatorUserId);
    Task<List<OperatorOfficeResponse>> GetMyOfficesAsync(Guid operatorUserId);
    Task AddOfficeAsync(Guid operatorUserId, AddOfficeRequest request);
    Task RemoveOfficeAsync(Guid operatorUserId, Guid officeId);
    Task RemoveVehicleAsync(Guid operatorUserId, Guid vehicleId);
    Task ScheduleMaintenanceAsync(Guid operatorUserId, Guid busId, DateTime? start, DateTime? end);
}
