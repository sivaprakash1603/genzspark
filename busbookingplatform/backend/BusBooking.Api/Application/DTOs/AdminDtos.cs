namespace BusBooking.Api.Application.DTOs;

public record OperatorStatusUpdateRequest(string Reason);
public record OperatorReviewResponse(Guid OperatorUserId, string Username, string Email, string ApprovalStatus, bool IsEnabled);
public record PlatformFeeResponse(Guid Id, string FeeType, decimal Value, bool IsActive, DateTime EffectiveFrom, DateTime CreatedAt);
public record UpdatePlatformFeeRequest(string FeeType, decimal Value);
public record EmailLogResponse(Guid Id, Guid? UserId, string ToEmail, string Subject, string TemplateKey, string Status, string? ErrorMessage, DateTime CreatedAt);

public record AdminStatsResponse(
    decimal TotalRevenue,
    decimal TotalRefunds,
    decimal NetRevenue,
    int ActiveBuses,
    int TotalBookings,
    int TotalOperators,
    int TotalUsers,
    List<RouteRevenueDto> TopRoutes
);

public record RouteRevenueDto(string RouteName, decimal Revenue, int BookingCount);
