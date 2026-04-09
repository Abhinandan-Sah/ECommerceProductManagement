using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Domain.Exceptions;

namespace Identity.API.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepo, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int pageSize, string? search, string? role)
        {
            _logger.LogInformation("Fetching users (Page: {Page}, PageSize: {PageSize})", page, pageSize);

            var users = await _userRepo.GetAllUsersAsync(page, pageSize, search, role);
            return users.Select(MapToDto);
        }

        public async Task<int> GetUsersCountAsync(string? search, string? role)
        {
            return await _userRepo.GetUsersCountAsync(search, role);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching user {UserId}", id);

            var user = await _userRepo.GetUserByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId)
        {
            return await _userRepo.IsEmailTakenAsync(email, excludeUserId);
        }

        public async Task<UserResponseDto> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request)
        {
            _logger.LogInformation("Updating profile for user {UserId}", id);

            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateUserAsync(user);

            _logger.LogInformation("Profile updated for user {UserId}", id);
            return MapToDto(user);
        }

        public async Task UpdateUserRoleAsync(Guid id, Role role)
        {
            _logger.LogInformation("Updating role for user {UserId} to {Role}", id, role);

            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepo.UpdateUserAsync(user);

            _logger.LogInformation("Role updated for user {UserId}", id);
        }

        public async Task SetUserActiveAsync(Guid id, bool isActive)
        {
            _logger.LogInformation("Setting user {UserId} active status to {IsActive}", id, isActive);

            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (!isActive)
            {
                await _userRepo.RevokeRefreshTokensAsync(id, "Account deactivated");
            }

            await _userRepo.UpdateUserAsync(user);

            _logger.LogInformation("User {UserId} active status set to {IsActive}", id, isActive);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            _logger.LogInformation("Deleting user {UserId}", id);

            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            await _userRepo.DeleteUserAsync(user);

            _logger.LogInformation("User {UserId} deleted successfully", id);
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
