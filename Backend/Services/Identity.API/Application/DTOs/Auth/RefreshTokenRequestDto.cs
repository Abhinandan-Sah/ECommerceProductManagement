using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
