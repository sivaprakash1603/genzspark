namespace BusBooking.Api.Domain.Entities;

internal class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }
    public virtual User? Operator { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string BusName { get; set; } = string.Empty;
    public string SeatLayoutType { get; set; } = "2x2";
    public int TotalSeats { get; set; } = 40;
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();

    public override string ToString()
    {
        return $"Vehicle: {Id} | Name: {BusName} | Number: {VehicleNumber} | Seats: {TotalSeats}";
    }
}
