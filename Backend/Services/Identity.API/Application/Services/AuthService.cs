using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IUserRepository _userRepo;
        private readonly JwtTokenGenerator _jwtGenerator;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepo, IUserRepository userRepo, JwtTokenGenerator jwtGenerator, IConfiguration config)
        {
            _authRepo = authRepo;
            _userRepo = userRepo;
            _jwtGenerator = jwtGenerator;
            _config = config;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _authRepo.EmailExistsAsync(email);
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Role = Role.Customer,
            };

            await _authRepo.AddUserAsync(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _authRepo.GetUserByEmailAsync(request.Email);

            if (user == null) return null;
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash)) return null;
            if (!user.IsActive) return null;

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var stored = await _authRepo.GetRefreshTokenAsync(refreshToken);

            if (stored == null || !stored.IsActive) return null;
            if (!stored.User.IsActive) return null;

            stored.IsRevoked = true;
            stored.RevokedReason = "Replaced by new token";

            return await IssueTokensAsync(stored.User);
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var stored = await _authRepo.GetRefreshTokenAsync(refreshToken);

            if (stored == null || !stored.IsActive) return false;

            stored.IsRevoked = true;
            stored.RevokedReason = "User logged out";
            
            await _authRepo.SaveChangesAsync();
            return true;
        }

        public async Task<string?> ForgotPasswordAsync(string email)
        {
            var user = await _authRepo.GetUserByEmailAsync(email);
            if (user == null) return null;

            var existing = await _authRepo.GetActiveResetTokensAsync(user.Id);
            foreach (var t in existing)
            {
                t.IsUsed = true;
            }

            var resetToken = new PasswordResetToken
            {
                Token = JwtTokenGenerator.GenerateRefreshToken(),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            };

            await _authRepo.AddResetTokenAsync(resetToken);
            await _authRepo.SaveChangesAsync();

            return resetToken.Token;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var tokenRecord = await _authRepo.GetResetTokenAsync(request.Token);

            if (tokenRecord == null || !tokenRecord.IsValid) return false;

            tokenRecord.User.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            tokenRecord.User.UpdatedAt = DateTime.UtcNow;
            tokenRecord.IsUsed = true;

            await _authRepo.RevokeAllRefreshTokensAsync(tokenRecord.UserId, "Password was reset");
            await _authRepo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null) return false;

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash)) return false;

            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _authRepo.RevokeAllRefreshTokensAsync(userId, "Password changed");
            await _userRepo.UpdateUserAsync(user);

            return true;
        }

        public async Task<UserResponseDto?> GetProfileAsync(Guid userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            return user == null ? null : MapToDto(user);
        }

        private async Task<AuthResponseDto> IssueTokensAsync(User user)
        {
            var expiryDays = _config.GetValue<int>("RefreshTokenSettings:ExpiryDays", 7);
            var refreshToken = JwtTokenGenerator.GenerateRefreshToken();

            await _authRepo.AddRefreshTokenAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            });

            await _authRepo.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = _jwtGenerator.GenerateToken(user),
                RefreshToken = refreshToken,
                ExpiresAt = _jwtGenerator.GetTokenExpiry(),
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString()
            };
        }

        private static UserResponseDto MapToDto(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
