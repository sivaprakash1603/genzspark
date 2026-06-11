using BusBooking.Api.Domain.Enums;

namespace BusBooking.Api.Domain.Entities;

internal class Refund
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
    public RefundStatus RefundStatus { get; set; } = RefundStatus.Pending;
    public decimal RefundAmount { get; set; }
    public decimal RefundPercentage { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Notes { get; set; }

    public override string ToString()
    {
        return $"Refund: {Id} | Booking: {BookingId} | Amount: INR {RefundAmount:N2} | Status: {RefundStatus}";
    }
}
