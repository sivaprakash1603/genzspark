namespace BusBooking.Api.Domain.Entities;

public class BookingSeat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Booking? Booking { get; set; }
    public Guid SeatId { get; set; }
    public Seat? Seat { get; set; }
    public decimal Fare { get; set; }
}
