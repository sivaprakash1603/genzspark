namespace BusBooking.Api.Domain.Entities;

public class OperatorOffice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }
    public virtual User? Operator { get; set; }
    
    public string CityName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
