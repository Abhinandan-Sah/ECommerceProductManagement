using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;

namespace Identity.API.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int pageSize, string? search, string? role)
        {
            var users = await _userRepo.GetAllUsersAsync(page, pageSize, search, role);
            return users.Select(MapToDto);
        }

        public async Task<int> GetUsersCountAsync(string? search, string? role)
        {
            return await _userRepo.GetUsersCountAsync(search, role);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId)
        {
            return await _userRepo.IsEmailTakenAsync(email, excludeUserId);
        }

        public async Task<UserResponseDto> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) throw new InvalidOperationException("User not found.");

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateUserAsync(user);
            return MapToDto(user);
        }

        public async Task UpdateUserRoleAsync(Guid id, Role role)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return;

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepo.UpdateUserAsync(user);
        }

        public async Task SetUserActiveAsync(Guid id, bool isActive)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (!isActive)
            {
                await _userRepo.RevokeRefreshTokensAsync(id, "Account deactivated");
            }

            await _userRepo.UpdateUserAsync(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return;

            await _userRepo.DeleteUserAsync(user);
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
