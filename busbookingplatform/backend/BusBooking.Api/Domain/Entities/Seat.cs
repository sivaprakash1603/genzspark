namespace BusBooking.Api.Domain.Entities;

public class Seat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BusId { get; set; }
    public Bus? Bus { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string? SeatType { get; set; }
    public bool IsActive { get; set; } = true;

    // Seat Locking for Concurrency
    public DateTime? LockedUntil { get; set; }
    public Guid? LockedByUserId { get; set; }
}
