namespace BusBooking.Api.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Booking? Booking { get; set; }
    public Guid TransactionId { get; set; }
    public string PaymentStatus { get; set; } = "Failed";
    public decimal Amount { get; set; }
    public string Gateway { get; set; } = "Dummy";
    public string? CardLast4 { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public string? ResponsePayload { get; set; }
}
