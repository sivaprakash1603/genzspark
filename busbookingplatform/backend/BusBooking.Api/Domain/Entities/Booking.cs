namespace BusBooking.Api.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PassengerId { get; set; }
    public virtual User? Passenger { get; set; }
    public Guid BusId { get; set; }
    public virtual Bus? Bus { get; set; }
    public string BookingStatus { get; set; } = "PendingPayment";
    public decimal TotalAmount { get; set; }
    public DateTime JourneyDate { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation properties
    public virtual ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    public virtual ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
