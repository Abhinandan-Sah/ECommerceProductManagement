using Identity.API.Application.DTOs.User;
using Identity.API.Domain.Enums;

namespace Identity.API.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int pageSize, string? search, string? role);
        Task<int> GetUsersCountAsync(string? search, string? role);
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);                   
        Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId); 
        Task<UserResponseDto> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request);
        Task UpdateUserRoleAsync(Guid id, Role role);
        Task SetUserActiveAsync(Guid id, bool isActive);
        Task DeleteUserAsync(Guid id);
    }
}
