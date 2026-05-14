using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    /// <summary>
    /// Captures the current and replacement password for an authenticated user.
    /// </summary>
    public class ChangePasswordRequestDto
    {
        /// <summary>
        /// Current plain password used to prove account ownership.
        /// </summary>
        [Required]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string CurrentPassword { get; set; } = string.Empty;

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
