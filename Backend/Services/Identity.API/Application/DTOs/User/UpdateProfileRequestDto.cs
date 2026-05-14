using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.User
{
    /// <summary>
    /// Captures editable profile details for the authenticated user.
    /// </summary>
    public class UpdateProfileRequestDto
    {
        /// <summary>
        /// Updated display name for the user.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Updated unique email address for login and account recovery.
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
    }
}
