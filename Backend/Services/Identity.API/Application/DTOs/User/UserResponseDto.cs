namespace Identity.API.Application.DTOs.User
{
    /// <summary>
    /// Represents user account data returned by identity endpoints.
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public Guid      Id        { get; set; }

        /// <summary>
        /// User's display name.
        /// </summary>
        public string    FullName  { get; set; } = string.Empty;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string    Email     { get; set; } = string.Empty;

        /// <summary>
        /// User's role name.
        /// </summary>
        public string    Role      { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the user account can sign in.
        /// </summary>
        public bool      IsActive  { get; set; }

        /// <summary>
        /// UTC time when the account was created.
        /// </summary>
        public DateTime  CreatedAt { get; set; }

        /// <summary>
        /// UTC time when the account was last updated, when available.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
