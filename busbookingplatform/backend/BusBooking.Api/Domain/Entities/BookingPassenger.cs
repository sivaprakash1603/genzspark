namespace BusBooking.Api.Domain.Entities;

internal class BookingPassenger
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
    public Guid SeatId { get; set; }
    public virtual Seat? Seat { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
}
