using BusBooking.Api.Domain.Enums;

namespace BusBooking.Api.Domain.Entities;

internal class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
    public Guid TransactionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Failed;
    public decimal Amount { get; set; }
    public string Gateway { get; set; } = "Dummy";
    public string? CardLast4 { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public string? ResponsePayload { get; set; }

    public override string ToString()
    {
        return $"Payment: {Id} | Booking: {BookingId} | Amount: INR {Amount:N2} | Status: {PaymentStatus}";
    }
}
