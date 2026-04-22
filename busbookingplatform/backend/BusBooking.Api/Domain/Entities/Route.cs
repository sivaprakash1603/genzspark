namespace BusBooking.Api.Domain.Entities;

public class Route
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourceId { get; set; }
    public SourceLocation? Source { get; set; }
    public Guid DestinationId { get; set; }
    public DestinationLocation? Destination { get; set; }
    public bool IsActive { get; set; } = true;
}
