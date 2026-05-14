namespace Identity.API.Application.DTOs.Auth
{
    /// <summary>
    /// Represents the tokens and user details returned after successful authentication.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// JWT access token used to call protected APIs.
        /// </summary>
        public string AccessToken  { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token used to renew the session.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// UTC time when the access token expires.
        /// </summary>
        public DateTime ExpiresAt  { get; set; }

        /// <summary>
        /// Authenticated user's email address.
        /// </summary>
        public string Email        { get; set; } = string.Empty;

        /// <summary>
        /// Authenticated user's display name.
        /// </summary>
        public string FullName     { get; set; } = string.Empty;

        /// <summary>
        /// Authenticated user's role name.
        /// </summary>
        public string Role         { get; set; } = string.Empty;
    }
}
