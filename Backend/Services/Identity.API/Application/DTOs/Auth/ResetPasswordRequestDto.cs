using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    /// <summary>
    /// Captures the reset token and replacement password.
    /// </summary>
    public class ResetPasswordRequestDto
    {
        /// <summary>
        /// Password reset token previously issued to the user.
        /// </summary>
        [Required]
        [MaxLength(500, ErrorMessage = "Token cannot exceed 500 characters")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// New plain password that will be hashed before storage.
        /// </summary>
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation value that must match the new password.
        /// </summary>
        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
