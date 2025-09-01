using System;
using System.Security.Claims;

namespace DevPulse.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            // Prefer NameIdentifier (nameid); fall back to "sub"
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(raw))
                throw new UnauthorizedAccessException("Missing user id claim.");

            if (!Guid.TryParse(raw, out var id))
                throw new UnauthorizedAccessException("Invalid user id claim.");

            return id;
        }
    }
}