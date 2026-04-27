namespace BusBooking.Api.Domain.Entities;

public class PlatformFeeConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FeeType { get; set; } = "Fixed";
    public decimal Value { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
