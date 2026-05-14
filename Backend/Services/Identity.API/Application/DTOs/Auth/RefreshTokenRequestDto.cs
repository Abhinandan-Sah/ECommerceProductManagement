using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs.Auth
{
    /// <summary>
    /// Captures a refresh token submitted by the client.
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Refresh token value to rotate or revoke.
        /// </summary>
        [Required]
        [MaxLength(500, ErrorMessage = "Refresh token cannot exceed 500 characters")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
