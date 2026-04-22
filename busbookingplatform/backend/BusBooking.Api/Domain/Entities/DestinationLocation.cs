namespace BusBooking.Api.Domain.Entities;

public class DestinationLocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
