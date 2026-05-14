using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Repositories
{
    /// <summary>
    /// Uses the identity database to read and update user account records.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDBContext _context;

        /// <summary>
        /// Creates the user repository for the current identity database context.
        /// </summary>
        /// <param name="context">Identity database context used for user account data.</param>
        public UserRepository(IdentityDBContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <remarks>Builds the EF Core query from optional search and role filters before applying paging.</remarks>
        public async Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize, string? search, string? role)
        {
            // Build one query first so filtering and paging happen in the database.
            var query = BuildFilterQuery(search, role);

            return await query
                .OrderBy(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetUsersCountAsync(string? search, string? role)
        {
            return await BuildFilterQuery(search, role).CountAsync();
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId);
        }

        /// <inheritdoc />
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task RevokeRefreshTokensAsync(Guid userId, string reason)
        {
            // Revoking refresh tokens stops future token rotation for existing sessions.
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();
            
            tokens.ForEach(r => { r.IsRevoked = true; r.RevokedReason = reason; });
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, int>> GetRoleBreakdownAsync()
        {
            return await _context.Users
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Role, x => x.Count);
        }

        /// <summary>
        /// Builds the shared user filter query used by listing and count operations.
        /// </summary>
        private IQueryable<User> BuildFilterQuery(string? search, string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<Role>(role, true, out var parsedRole))
                query = query.Where(u => u.Role == parsedRole);

            return query;
        }
    }
}
