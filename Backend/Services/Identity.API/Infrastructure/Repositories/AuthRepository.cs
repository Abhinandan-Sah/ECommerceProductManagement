using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Domain.Entities;
using Identity.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IdentityDBContext _context;

        public AuthRepository(IdentityDBContext context)
        {
            _context = context;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
        }

        public async Task RevokeAllRefreshTokensAsync(Guid userId, string reason)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();
            tokens.ForEach(r => { r.IsRevoked = true; r.RevokedReason = reason; });
        }

        public async Task<IEnumerable<PasswordResetToken>> GetActiveResetTokensAsync(Guid userId)
        {
            return await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task AddResetTokenAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Add(token);
        }

        public async Task<PasswordResetToken?> GetResetTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
