namespace BusBooking.Api.Domain.Entities;

internal class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public virtual Role? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<OperatorProfile> OperatorProfiles { get; set; } = new List<OperatorProfile>();
    public virtual ICollection<OperatorOffice> OperatorOffices { get; set; } = new List<OperatorOffice>();

    public override string ToString()
    {
        return $"User: {Id} | Username: {Username} | Email: {Email} | RoleId: {RoleId}";
    }
}
