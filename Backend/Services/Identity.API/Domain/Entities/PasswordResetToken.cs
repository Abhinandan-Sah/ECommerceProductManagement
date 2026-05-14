namespace Identity.API.Domain.Entities
{
    /// <summary>
    /// Represents a short-lived token used to reset a user's password.
    /// </summary>
    public class PasswordResetToken
    {
        /// <summary>
        /// Unique password reset token record identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Opaque reset token value sent to the user.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        // Foreign key
        /// <summary>
        /// User identifier that owns the reset token.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User navigation property.
        /// </summary>
        public User User { get; set; } = null!;

        // Expires in 30 minutes by default
        /// <summary>
        /// UTC time when the reset token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);

        /// <summary>
        /// UTC time when the reset token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the reset token has already been used.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        // Computed helpers
        /// <summary>
        /// Indicates whether the reset token has passed its expiry time.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Indicates whether the reset token can still reset a password.
        /// </summary>
        public bool IsValid => !IsUsed && !IsExpired;
    }
}
