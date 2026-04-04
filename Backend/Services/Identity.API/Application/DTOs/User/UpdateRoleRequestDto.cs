using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.User
{
    public class UpdateRoleRequestDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
