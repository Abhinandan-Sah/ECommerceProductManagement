using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    public class ChangePasswordRequestDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
