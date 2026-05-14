using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    /// <summary>
    /// Captures the email address requesting password reset.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        /// Email address that should receive reset instructions when registered.
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public string Email { get; set; } = string.Empty;
    }
}
