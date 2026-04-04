using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
