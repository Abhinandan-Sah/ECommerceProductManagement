using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces;
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

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(
            int page, int pageSize, string? search, string? role)
        {
            var query = BuildFilterQuery(search, role);

            return await query
                .OrderBy(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => MapToDto(u))
                .ToListAsync();
        }

        public async Task<int> GetUsersCountAsync(string? search, string? role)
        {
            return await BuildFilterQuery(search, role).CountAsync();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId);
        }

        public async Task<UserResponseDto> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request)
        {
            var user = await _context.Users.FindAsync(id);

            user!.FullName  = request.FullName;
            user.Email     = request.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task UpdateUserRoleAsync(Guid id, Role role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return;

            user.Role      = role;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task SetUserActiveAsync(Guid id, bool isActive)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return;

            user.IsActive  = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            // Revoke all refresh tokens when deactivating (force logout everywhere)
            if (!isActive)
            {
                var tokens = await _context.RefreshTokens
                    .Where(r => r.UserId == id && !r.IsRevoked)
                    .ToListAsync();
                tokens.ForEach(r => { r.IsRevoked = true; r.RevokedReason = "Account deactivated"; });
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────

        private IQueryable<User> BuildFilterQuery(string? search, string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<Role>(role, true, out var parsedRole))
                query = query.Where(u => u.Role == parsedRole);

            return query;
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
