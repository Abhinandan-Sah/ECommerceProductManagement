using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines tracked user-account persistence operations for profile, administration, and reporting screens.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a page of users after optional search and role filters have been applied.
        /// </summary>
        /// <param name="page">One-based page number to return.</param>
        /// <param name="pageSize">Maximum number of users to include on each page.</param>
        /// <param name="search">Optional name or email text to search for.</param>
        /// <param name="role">Optional role name to filter by.</param>
        /// <returns>Tracked users that match the requested page and filters, or an empty collection when none match.</returns>
        Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize, string? search, string? role);

        /// <summary>
        /// Counts users after optional search and role filters have been applied.
        /// </summary>
        /// <param name="search">Optional name or email text to search for.</param>
        /// <param name="role">Optional role name to filter by.</param>
        /// <returns>Total number of matching users.</returns>
        Task<int> GetUsersCountAsync(string? search, string? role);

        /// <summary>
        /// Finds a user by identifier.
        /// </summary>
        /// <param name="id">User identifier to load.</param>
        /// <returns>The tracked matching user, or null when no account exists.</returns>
        Task<User?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Checks whether an email is already used by another account.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <param name="excludeUserId">Current user identifier to ignore during the check.</param>
        /// <returns>True when another user already owns the email address.</returns>
        Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId);

        /// <summary>
        /// Updates a user account and saves the database change immediately.
        /// </summary>
        /// <param name="user">User entity with updated values.</param>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// Deletes an existing user account and saves the database change immediately.
        /// </summary>
        /// <param name="user">User entity to remove.</param>
        Task DeleteUserAsync(User user);

        /// <summary>
        /// Revokes active refresh tokens for a user and saves the database change immediately.
        /// </summary>
        /// <param name="userId">User whose refresh tokens should be revoked.</param>
        /// <param name="reason">Reason recorded against each revoked token.</param>
        Task RevokeRefreshTokensAsync(Guid userId, string reason);

        /// <summary>
        /// Counts users by role for administration dashboards.
        /// </summary>
        /// <returns>A dictionary where each key is a role name and each value is the user count, or an empty dictionary when no users exist.</returns>
        Task<Dictionary<string, int>> GetRoleBreakdownAsync();
    }
}
