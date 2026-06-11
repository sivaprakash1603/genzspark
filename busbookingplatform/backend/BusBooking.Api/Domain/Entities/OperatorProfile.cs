using BusBooking.Api.Domain.Enums;

namespace BusBooking.Api.Domain.Entities;

internal class OperatorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public bool IsEnabled { get; set; } = false;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public override string ToString()
    {
        return $"OperatorProfile: {Id} | User: {UserId} | Status: {ApprovalStatus}";
    }
}
