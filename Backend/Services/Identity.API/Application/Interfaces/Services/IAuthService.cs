using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;

namespace Identity.API.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<bool> EmailExistsAsync(string email);
        Task RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);          
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);       
        Task<bool> LogoutAsync(string refreshToken);             
        Task<string?> ForgotPasswordAsync(string email);            
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);   
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request); 
        Task<UserResponseDto?> GetProfileAsync(Guid userId);                 
    }
}
