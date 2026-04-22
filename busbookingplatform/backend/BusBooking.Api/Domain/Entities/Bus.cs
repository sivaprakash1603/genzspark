namespace BusBooking.Api.Domain.Entities;

public class Bus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }
    public User? Operator { get; set; }
    public Guid RouteId { get; set; }
    public Route? Route { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string BoardingPoint { get; set; } = string.Empty;
    public string DropPoint { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal TotalPrice { get; set; }
    public string SeatLayoutType { get; set; } = "2x2";
    public int TotalSeats { get; set; }
    public string ApprovalStatus { get; set; } = "Pending";
    public bool IsActive { get; set; } = true;
    public bool IsTemporarilyDisabled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
