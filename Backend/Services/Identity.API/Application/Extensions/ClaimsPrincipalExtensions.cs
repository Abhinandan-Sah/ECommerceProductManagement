using System.Security.Claims;

namespace Identity.API.Application.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Safely extracts the user ID (NameIdentifier claim) from the current ClaimsPrincipal.
        /// </summary>
        public static Guid? GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return value == null ? null : Guid.Parse(value);
        }
    }
}
