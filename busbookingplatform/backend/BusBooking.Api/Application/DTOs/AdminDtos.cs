namespace BusBooking.Api.Application.DTOs;

public record OperatorStatusUpdateRequest(string Reason);
public record OperatorReviewResponse(Guid OperatorUserId, string Username, string Email, string VehicleNumber, string ApprovalStatus, bool IsEnabled);
