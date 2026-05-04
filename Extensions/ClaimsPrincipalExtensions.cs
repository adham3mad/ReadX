using System.Security.Claims;

namespace ReadX.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("sub");
    }
}
