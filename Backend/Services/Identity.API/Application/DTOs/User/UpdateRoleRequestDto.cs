using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.User
{
    /// <summary>
    /// Captures the role an administrator wants to assign to a user.
    /// </summary>
    public class UpdateRoleRequestDto
    {
        /// <summary>
        /// Role name that must match a known identity role.
        /// </summary>
        [Required]
        [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string Role { get; set; } = string.Empty;
    }
}
