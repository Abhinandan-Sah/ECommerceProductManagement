using Identity.API.Domain.Enums;

namespace Identity.API.Domain.Entities
{
    /// <summary>
    /// Represents an application user account and its security state.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User's display name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Unique email address used for login and account recovery.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password value; the plain password is never stored.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Role that controls authorization across the system.
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// Indicates whether the account can sign in and refresh sessions.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// UTC time when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC time when the user account was last updated, when available.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        /// <summary>
        /// Refresh tokens issued to this user.
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>
        /// Password reset tokens issued to this user.
        /// </summary>
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    }
}
