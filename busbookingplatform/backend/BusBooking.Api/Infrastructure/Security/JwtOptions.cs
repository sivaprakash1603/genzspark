namespace BusBooking.Api.Infrastructure.Security;

public class JwtOptions
{
    public string Issuer { get; set; } = "BusBooking.Api";
    public string Audience { get; set; } = "BusBooking.Client";
    public string Secret { get; set; } = "replace-this-secret";
    public int ExpiryMinutes { get; set; } = 120;
}
