using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IAdminService
{
    Task<List<OperatorReviewResponse>> GetPendingOperatorsAsync();
    Task ApproveOperatorAsync(Guid operatorId);
    Task RejectOperatorAsync(Guid operatorId, string reason);
    Task EnableOperatorAsync(Guid operatorId);
    Task DisableOperatorAsync(Guid operatorId);
    Task ApproveBusAsync(Guid busId);
    Task RejectBusAsync(Guid busId, string reason);
    Task<RouteResponse> CreateRouteAsync(CreateRouteRequest request);
    Task<RouteResponse> CreateSourceAsync(CreateSourceRequest request);
    Task<RouteResponse> CreateDestinationAsync(CreateDestinationRequest request);
}
