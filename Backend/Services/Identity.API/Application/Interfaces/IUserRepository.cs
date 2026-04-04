using Identity.API.Application.DTOs.User;
using Identity.API.Domain.Enums;

namespace Identity.API.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int pageSize, string? search, string? role);
        Task<int>              GetUsersCountAsync(string? search, string? role);
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);                   // null = not found
        Task<bool>             IsEmailTakenAsync(string email, Guid excludeUserId); // true = already taken
        Task<UserResponseDto>  UpdateProfileAsync(Guid id, UpdateProfileRequestDto request);
        Task                   UpdateUserRoleAsync(Guid id, Role role);
        Task                   SetUserActiveAsync(Guid id, bool isActive);
        Task                   DeleteUserAsync(Guid id);
    }
}
