using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Infrastructure.Data;
using Identity.API.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IdentityDBContext _context;
        private readonly JwtTokenGenerator _jwtGenerator;
        private readonly IConfiguration _config;

        public AuthRepository(IdentityDBContext context, JwtTokenGenerator jwtGenerator, IConfiguration config)
        {
            _context      = context;
            _jwtGenerator = jwtGenerator;
            _config       = config;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            var user = new User
            {
                FullName     = request.FullName,
                Email        = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Role         = Role.Customer,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null) return null;
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash)) return null;
            if (!user.IsActive) return null;

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (stored == null || !stored.IsActive) return null;
            if (!stored.User.IsActive) return null;

            // Rotate: revoke old token and issue a new pair
            stored.IsRevoked     = true;
            stored.RevokedReason = "Replaced by new token";

            return await IssueTokensAsync(stored.User);
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (stored == null || !stored.IsActive) return false;

            stored.IsRevoked     = true;
            stored.RevokedReason = "User logged out";
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            // Invalidate any previously active reset tokens
            var existing = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            existing.ForEach(t => t.IsUsed = true);

            var resetToken = new PasswordResetToken
            {
                Token     = JwtTokenGenerator.GenerateRefreshToken(),
                UserId    = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            return resetToken.Token;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var tokenRecord = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.Token);

            if (tokenRecord == null || !tokenRecord.IsValid) return false;

            tokenRecord.User.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            tokenRecord.User.UpdatedAt    = DateTime.UtcNow;
            tokenRecord.IsUsed            = true;

            await RevokeAllRefreshTokensAsync(tokenRecord.UserId, "Password was reset");
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash)) return false;

            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt    = DateTime.UtcNow;

            await RevokeAllRefreshTokensAsync(userId, "Password changed");
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UserResponseDto?> GetProfileAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? null : MapToDto(user);
        }

        // ─────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────

        private async Task<AuthResponseDto> IssueTokensAsync(User user)
        {
            // Dynamically read the 7-day limit from appsettings.json, defaulting to 7 if missing
            var expiryDays = _config.GetValue<int>("RefreshTokenSettings:ExpiryDays", 7);
            var refreshToken = JwtTokenGenerator.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token     = refreshToken,
                UserId    = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            });

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken  = _jwtGenerator.GenerateToken(user),
                RefreshToken = refreshToken,
                ExpiresAt    = _jwtGenerator.GetTokenExpiry(),
                Email        = user.Email,
                FullName     = user.FullName,
                Role         = user.Role.ToString()
            };
        }

        private async Task RevokeAllRefreshTokensAsync(Guid userId, string reason)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();
            tokens.ForEach(r => { r.IsRevoked = true; r.RevokedReason = reason; });
        }

        private static UserResponseDto MapToDto(User user) => new()
        {
            Id        = user.Id,
            FullName  = user.FullName,
            Email     = user.Email,
            Role      = user.Role.ToString(),
            IsActive  = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
