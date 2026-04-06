using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task AddUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken token);
        Task RevokeAllRefreshTokensAsync(Guid userId, string reason);

        Task<IEnumerable<PasswordResetToken>> GetActiveResetTokensAsync(Guid userId);
        Task AddResetTokenAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetResetTokenAsync(string token);

        Task SaveChangesAsync();
    }
}
