using System.Security.Claims;

namespace DevPulse.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException("User id claim missing.");

        return Guid.Parse(id);
    }
}