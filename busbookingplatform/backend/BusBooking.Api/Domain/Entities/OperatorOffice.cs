namespace BusBooking.Api.Domain.Entities;

internal class OperatorOffice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }
    public virtual User? Operator { get; set; }
    
    public string CityName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
