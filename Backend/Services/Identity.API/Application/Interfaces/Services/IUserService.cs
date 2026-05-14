using Identity.API.Application.DTOs.User;
using Identity.API.Domain.Enums;

namespace Identity.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines user profile and administrator account-management operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets a page of users after optional search and role filters have been applied.
        /// </summary>
        /// <param name="page">Page number to return.</param>
        /// <param name="pageSize">Number of users to include on each page.</param>
        /// <param name="search">Optional name or email text to search for.</param>
        /// <param name="role">Optional role name to filter by.</param>
        /// <returns>Users that match the requested page and filters.</returns>
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int pageSize, string? search, string? role);

        /// <summary>
        /// Counts users after optional search and role filters have been applied.
        /// </summary>
        /// <param name="search">Optional name or email text to search for.</param>
        /// <param name="role">Optional role name to filter by.</param>
        /// <returns>Total number of matching users.</returns>
        Task<int> GetUsersCountAsync(string? search, string? role);

        /// <summary>
        /// Gets a user's public account profile.
        /// </summary>
        /// <param name="id">User identifier to load.</param>
        /// <returns>The matching user profile, or null when no account exists.</returns>
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);                   

        /// <summary>
        /// Checks whether another account already uses the email address.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <param name="excludeUserId">Current user identifier to ignore during the check.</param>
        /// <returns>True when another account owns the email address.</returns>
        Task<bool> IsEmailTakenAsync(string email, Guid excludeUserId); 

        /// <summary>
        /// Updates a user's own profile details.
        /// </summary>
        /// <param name="id">User identifier to update.</param>
        /// <param name="request">New profile values.</param>
        /// <returns>The updated user profile.</returns>
        Task<UserResponseDto> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request);

        /// <summary>
        /// Changes the role assigned to a user account.
        /// </summary>
        /// <param name="id">User identifier to update.</param>
        /// <param name="role">Role that should be assigned.</param>
        Task UpdateUserRoleAsync(Guid id, Role role);

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="id">User identifier to update.</param>
        /// <param name="isActive">True to activate the account; false to deactivate it.</param>
        Task SetUserActiveAsync(Guid id, bool isActive);

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="id">User identifier to delete.</param>
        Task DeleteUserAsync(Guid id);

        /// <summary>
        /// Gets user counts grouped by role.
        /// </summary>
        /// <returns>A dictionary where each key is a role name and each value is the user count.</returns>
        Task<Dictionary<string, int>> GetUserStatsAsync();
    }
}
