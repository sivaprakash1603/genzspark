namespace BusBooking.Api.Domain.Entities;

public class Route
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourceId { get; set; }
    public virtual Location? Source { get; set; }
    public Guid DestinationId { get; set; }
    public virtual Location? Destination { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();
}
