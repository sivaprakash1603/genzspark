namespace BusBooking.Api.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PassengerId { get; set; }
    public User? Passenger { get; set; }
    public Guid BusId { get; set; }
    public Bus? Bus { get; set; }
    public string BookingStatus { get; set; } = "PendingPayment";
    public decimal TotalAmount { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}
