using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDBContext _context;

        public UserRepository(IdentityDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize, string? search, string? role)
        {
            var query = BuildFilterQuery(search, role);

            return await query
                .OrderBy(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUsersCountAsync(string? search, string? role)
        {
            return await BuildFilterQuery(search, role).CountAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokensAsync(Guid userId, string reason)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();
            
            tokens.ForEach(r => { r.IsRevoked = true; r.RevokedReason = reason; });
            await _context.SaveChangesAsync();
        }

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
