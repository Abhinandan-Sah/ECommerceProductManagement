namespace Identity.API.Domain.Entities
{
    /// <summary>
    /// Represents a refresh token that can renew a user's session until it expires or is revoked.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Unique refresh token record identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Opaque refresh token value presented by the client.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        // Foreign key
        /// <summary>
        /// User identifier that owns the refresh token.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User navigation property.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// UTC time when the refresh token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// UTC time when the refresh token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the refresh token has been revoked before expiry.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Reason recorded when the token is revoked.
        /// </summary>
        public string? RevokedReason { get; set; }

        // Computed helpers
        /// <summary>
        /// Indicates whether the refresh token has passed its expiry time.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Indicates whether the refresh token can still be used.
        /// </summary>
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
