using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize, string? search, string? role);
        Task<int> GetUsersCountAsync(string? search, string? role);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task RevokeRefreshTokensAsync(Guid userId, string reason);
    }
}
