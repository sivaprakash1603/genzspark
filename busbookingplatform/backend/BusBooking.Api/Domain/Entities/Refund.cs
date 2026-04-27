namespace BusBooking.Api.Domain.Entities;

public class Refund
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
    public string RefundStatus { get; set; } = "Pending";
    public decimal RefundAmount { get; set; }
    public decimal RefundPercentage { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Notes { get; set; }
}
