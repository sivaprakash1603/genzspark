namespace BusBooking.Api.Domain.Entities;

internal class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string? DownloadUrl { get; set; }

    public override string ToString()
    {
        return $"Ticket: {Id} | Number: {TicketNumber} | Booking: {BookingId}";
    }
}
