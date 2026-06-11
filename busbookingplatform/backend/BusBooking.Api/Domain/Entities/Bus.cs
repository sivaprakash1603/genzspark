using BusBooking.Api.Domain.Enums;

namespace BusBooking.Api.Domain.Entities;

internal class Bus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }
    public virtual User? Operator { get; set; }
    public Guid VehicleId { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
    public Guid RouteId { get; set; }
    public virtual Route? Route { get; set; }
    public string BoardingPoint { get; set; } = string.Empty;
    public string DropPoint { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public List<int> AvailableDays { get; set; } = new();
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal TotalPrice { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public bool IsActive { get; set; } = true;
    public bool IsTemporarilyDisabled { get; set; } = false;
    public bool IsMarkedForRemoval { get; set; } = false;
    public DateTime? RetirementDate { get; set; }
    public DateTime? MaintenanceStart { get; set; }
    public DateTime? MaintenanceEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public override string ToString()
    {
        return $"Bus: {Id} | Route: {RouteId} | Price: INR {TotalPrice:N2} | Status: {ApprovalStatus}";
    }
}
