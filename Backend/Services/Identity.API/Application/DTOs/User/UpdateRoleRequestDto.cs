using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.User
{
    public class UpdateRoleRequestDto
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string Role { get; set; } = string.Empty;
    }
}
