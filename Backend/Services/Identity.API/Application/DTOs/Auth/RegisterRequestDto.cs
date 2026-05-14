using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    /// <summary>
    /// Captures the details required to create a new customer account.
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// Display name for the new user.
        /// </summary>
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Unique email address used for login and account recovery.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Plain password that will be hashed before storage.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}
