using System.Security.Claims;

namespace ReadX.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// Helper method to easily retrieve the User ID (sub) from the JWT claims
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("sub");
    }
}
