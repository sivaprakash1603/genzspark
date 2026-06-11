using BusBooking.Api.Domain.Enums;

namespace BusBooking.Api.Domain.Entities;

internal class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PassengerId { get; set; }
    public virtual User? Passenger { get; set; }
    public Guid BusId { get; set; }
    public virtual Bus? Bus { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.PendingPayment;
    public decimal TotalAmount { get; set; }
    public DateTime JourneyDate { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public List<BookingSeat> BookingSeats { get; set; } = new();
    public List<BookingPassenger> BookingPassengers { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();
    public List<Refund> Refunds { get; set; } = new();

    public override string ToString()
    {
        return $"Booking: {Id} | Bus: {BusId} | Amount: INR {TotalAmount:N2} | Status: {BookingStatus}";
    }
}
