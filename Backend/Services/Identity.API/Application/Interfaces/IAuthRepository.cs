using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;

namespace Identity.API.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool>             EmailExistsAsync(string email);
        Task                   RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);          // null = wrong credentials
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);       // null = invalid token
        Task<bool>             LogoutAsync(string refreshToken);             // false = token not found
        Task<string?>          ForgotPasswordAsync(string email);            // null = email not found
        Task<bool>             ResetPasswordAsync(ResetPasswordRequestDto request);   // false = bad token
        Task<bool>             ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request); // false = wrong password
        Task<UserResponseDto?> GetProfileAsync(Guid userId);                 // null = not found
    }
}
