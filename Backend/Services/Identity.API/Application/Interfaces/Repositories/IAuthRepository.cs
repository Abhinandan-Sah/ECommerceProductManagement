using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines tracked authentication persistence operations for users, refresh tokens, and password reset tokens.
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// Checks whether the email address already belongs to an account.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True when a user with the email exists.</returns>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Adds a new user account and saves the database change immediately.
        /// </summary>
        /// <param name="user">User entity to create.</param>
        Task AddUserAsync(User user);

        /// <summary>
        /// Finds a user by email for login and account recovery.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The tracked matching user, or null when no account uses the email.</returns>
        Task<User?> GetUserByEmailAsync(string email);
        
        /// <summary>
        /// Loads a refresh token with its user so the token can be validated or revoked.
        /// </summary>
        /// <param name="token">Refresh token value supplied by the client.</param>
        /// <returns>The tracked stored refresh token, or null when the token is unknown.</returns>
        Task<RefreshToken?> GetRefreshTokenAsync(string token);

        /// <summary>
        /// Queues a refresh token to be stored for a user session.
        /// </summary>
        /// <param name="token">Refresh token entity to save.</param>
        Task AddRefreshTokenAsync(RefreshToken token);

        /// <summary>
        /// Revokes every active refresh token for a user without saving immediately.
        /// </summary>
        /// <param name="userId">User whose active sessions should be revoked.</param>
        /// <param name="reason">Reason recorded against each revoked token.</param>
        Task RevokeAllRefreshTokensAsync(Guid userId, string reason);

        /// <summary>
        /// Gets unused password reset tokens that have not expired for a user.
        /// </summary>
        /// <param name="userId">User whose reset tokens should be loaded.</param>
        /// <returns>Tracked active reset tokens for the user, or an empty collection when none exist.</returns>
        Task<IEnumerable<PasswordResetToken>> GetActiveResetTokensAsync(Guid userId);

        /// <summary>
        /// Queues a password reset token to be stored.
        /// </summary>
        /// <param name="token">Password reset token entity to save.</param>
        Task AddResetTokenAsync(PasswordResetToken token);

        /// <summary>
        /// Finds a password reset token and its user by the token value.
        /// </summary>
        /// <param name="token">Reset token value submitted by the user.</param>
        /// <returns>The tracked matching reset token, or null when it does not exist.</returns>
        Task<PasswordResetToken?> GetResetTokenAsync(string token);

        /// <summary>
        /// Persists any queued authentication token or account changes.
        /// </summary>
        Task SaveChangesAsync();
    }
}
