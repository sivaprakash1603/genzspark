using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IAdminService
{
    Task<List<OperatorReviewResponse>> GetPendingOperatorsAsync();
    Task ApproveOperatorAsync(Guid operatorId, Guid approvedByAdminId);
    Task RejectOperatorAsync(Guid operatorId, string reason);
    Task EnableOperatorAsync(Guid operatorId);
    Task DisableOperatorAsync(Guid operatorId, string reason);
    Task ApproveBusAsync(Guid busId);
    Task RejectBusAsync(Guid busId, string reason);
    Task<List<LocationResponse>> GetSourcesAsync();
    Task<List<LocationResponse>> GetDestinationsAsync();
    Task<List<RouteResponse>> GetRoutesAsync();
    Task<PlatformFeeResponse> GetPlatformFeeAsync();
    Task<PlatformFeeResponse> UpdatePlatformFeeAsync(UpdatePlatformFeeRequest request);
    Task<RouteResponse> CreateRouteAsync(CreateRouteRequest request);
    Task<RouteResponse> CreateRouteByNameAsync(CreateRouteByNameRequest request);
    Task<RouteResponse> CreateSourceAsync(CreateSourceRequest request);
    Task<RouteResponse> CreateDestinationAsync(CreateDestinationRequest request);
    Task<List<EmailLogResponse>> GetEmailLogsAsync();
    Task<AdminStatsResponse> GetAdminStatsAsync();
}
