using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    /// <summary>
    /// Captures the credentials required to sign in.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Email address used as the login name.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Plain password supplied for verification.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
