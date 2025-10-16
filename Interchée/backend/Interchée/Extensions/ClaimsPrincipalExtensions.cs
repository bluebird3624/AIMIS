using System.Security.Claims;

namespace Interchée.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static List<string> GetRoles(this ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }
    }
}