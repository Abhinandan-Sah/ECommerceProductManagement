using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Domain.Exceptions;
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
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthRepository authRepo, IUserRepository userRepo,
            JwtTokenGenerator jwtGenerator, IConfiguration config, ILogger<AuthService> logger)
        {
            _authRepo = authRepo;
            _userRepo = userRepo;
            _jwtGenerator = jwtGenerator;
            _config = config;
            _logger = logger;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _authRepo.EmailExistsAsync(email);
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("Registering new user with email: {Email}", request.Email);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Role = Role.Customer,
            };

            await _authRepo.AddUserAsync(user);

            _logger.LogInformation("User {UserId} registered successfully", user.Id);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _authRepo.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed — email not found: {Email}", request.Email);
                return null;
            }
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed — invalid password for email: {Email}", request.Email);
                return null;
            }
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed — account deactivated for email: {Email}", request.Email);
                return null;
            }

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);
            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Refreshing token");

            var stored = await _authRepo.GetRefreshTokenAsync(refreshToken);

            if (stored == null || !stored.IsActive) return null;
            if (!stored.User.IsActive) return null;

            stored.IsRevoked = true;
            stored.RevokedReason = "Replaced by new token";

            _logger.LogInformation("Token refreshed for user {UserId}", stored.UserId);
            return await IssueTokensAsync(stored.User);
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var stored = await _authRepo.GetRefreshTokenAsync(refreshToken);

            if (stored == null || !stored.IsActive) return false;

            stored.IsRevoked = true;
            stored.RevokedReason = "User logged out";
            
            await _authRepo.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged out successfully", stored.UserId);
            return true;
        }

        public async Task<string?> ForgotPasswordAsync(string email)
        {
            _logger.LogInformation("Forgot password request for email: {Email}", email);

            var user = await _authRepo.GetUserByEmailAsync(email);
            if (user == null)
            {
                // Return null but don't log anything to prevent email enumeration via logs
                return null;
            }

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

            // In development: Log the reset URL for testing
            // In production: This should be replaced with email service (SendGrid, SMTP)
            var resetUrl = $"https://app.example.com/reset-password?token={resetToken.Token}";
            _logger.LogWarning("PASSWORD RESET TOKEN (Development Only): User {UserId}, URL: {ResetUrl}", user.Id, resetUrl);
            
            // TODO: In production, send email with reset URL instead of logging
            // await _emailService.SendPasswordResetEmailAsync(user.Email, resetUrl);

            _logger.LogInformation("Password reset token generated for user {UserId}", user.Id);
            return resetToken.Token;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            _logger.LogInformation("Password reset attempt");

            var tokenRecord = await _authRepo.GetResetTokenAsync(request.Token);

            if (tokenRecord == null || !tokenRecord.IsValid) return false;

            tokenRecord.User.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            tokenRecord.User.UpdatedAt = DateTime.UtcNow;
            tokenRecord.IsUsed = true;

            await _authRepo.RevokeAllRefreshTokensAsync(tokenRecord.UserId, "Password was reset");
            await _authRepo.SaveChangesAsync();

            _logger.LogInformation("Password reset successfully for user {UserId}", tokenRecord.UserId);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            _logger.LogInformation("Password change attempt for user {UserId}", userId);

            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null) return false;

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash)) return false;

            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _authRepo.RevokeAllRefreshTokensAsync(userId, "Password changed");
            await _userRepo.UpdateUserAsync(user);

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return true;
        }

        public async Task<UserResponseDto?> GetProfileAsync(Guid userId)
        {
            _logger.LogInformation("Fetching profile for user {UserId}", userId);

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
