namespace BusBooking.Api.Domain.Entities;

public class BookingSeat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
    public Guid SeatId { get; set; }
    public virtual Seat? Seat { get; set; }
    public decimal Fare { get; set; }
}
