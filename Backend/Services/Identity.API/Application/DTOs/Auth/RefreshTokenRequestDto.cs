using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        [MaxLength(500, ErrorMessage = "Refresh token cannot exceed 500 characters")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
