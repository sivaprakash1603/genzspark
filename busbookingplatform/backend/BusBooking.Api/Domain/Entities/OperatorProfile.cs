namespace BusBooking.Api.Domain.Entities;

public class OperatorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = "Pending";
    public bool IsEnabled { get; set; } = false;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}
