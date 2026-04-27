namespace BusBooking.Api.Domain.Entities;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
