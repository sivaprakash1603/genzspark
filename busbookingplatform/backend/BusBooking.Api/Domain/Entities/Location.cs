namespace BusBooking.Api.Domain.Entities;

public class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Route> SourceRoutes { get; set; } = new List<Route>();
    public virtual ICollection<Route> DestinationRoutes { get; set; } = new List<Route>();
}
