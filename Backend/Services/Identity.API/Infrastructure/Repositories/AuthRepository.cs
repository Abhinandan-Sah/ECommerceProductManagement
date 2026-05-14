using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Domain.Entities;
using Identity.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Repositories
{
    /// <summary>
    /// Uses the identity database to persist authentication users and security tokens.
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly IdentityDBContext _context;

        /// <summary>
        /// Creates the authentication repository for the current identity database context.
        /// </summary>
        /// <param name="context">Identity database context used for authentication data.</param>
        public AuthRepository(IdentityDBContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <inheritdoc />
        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core tracking and includes the user navigation property.</remarks>
        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        /// <inheritdoc />
        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
        }

        /// <inheritdoc />
        public async Task RevokeAllRefreshTokensAsync(Guid userId, string reason)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();
            tokens.ForEach(r => { r.IsRevoked = true; r.RevokedReason = reason; });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PasswordResetToken>> GetActiveResetTokensAsync(Guid userId)
        {
            return await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task AddResetTokenAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Add(token);
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core tracking and includes the user navigation property.</remarks>
        public async Task<PasswordResetToken?> GetResetTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
