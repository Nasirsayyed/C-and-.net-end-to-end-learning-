using System.Security.Claims;

namespace ECommerceApi.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("No NameIdentifier claim present.");
        return Guid.Parse(value);
    }

    public static bool IsInRole(this ClaimsPrincipal principal, ECommerceApi.Models.UserRole role) =>
        principal.IsInRole(role.ToString());
}
