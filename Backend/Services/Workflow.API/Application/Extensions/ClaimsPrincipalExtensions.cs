using System.Security.Claims;

namespace Workflow.API.Application.Extensions
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal to extract user information from JWT tokens.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extracts the user ID from the JWT token claims.
        /// </summary>
        /// <param name="principal">The claims principal from the authenticated user.</param>
        /// <returns>The user ID as a Guid, or null if not found or invalid.</returns>
        public static Guid? GetUserId(this ClaimsPrincipal principal)
        {
            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }
    }
}
