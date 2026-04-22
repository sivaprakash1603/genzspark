using System.Security.Claims;

namespace BusBooking.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.Parse(id ?? throw new InvalidOperationException("User id claim not found"));
    }
}
