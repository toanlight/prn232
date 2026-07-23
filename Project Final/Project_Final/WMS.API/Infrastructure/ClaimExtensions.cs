using System.Security.Claims;

namespace WMS.API.Infrastructure;

/// <summary>
/// Extension methods to extract user info from JWT claims.
/// </summary>
public static class ClaimExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                    ?? user.FindFirst("sub");
        if (claim == null || !int.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");
        return userId;
    }

    public static string GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value
               ?? user.FindFirst("unique_name")?.Value
               ?? throw new UnauthorizedAccessException("Username not found in token.");
    }

    public static List<string> GetRoles(this ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }
}
