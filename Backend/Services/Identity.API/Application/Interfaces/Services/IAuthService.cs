using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;

namespace Identity.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines authentication and account-security operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Checks whether an email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True when the email already belongs to a user.</returns>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Registers a new customer account.
        /// </summary>
        /// <param name="request">Registration details from the client.</param>
        Task RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// Validates credentials and issues access and refresh tokens.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>Token response when credentials are valid; otherwise null.</returns>
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Rotates a valid refresh token and issues a new token pair.
        /// </summary>
        /// <param name="refreshToken">Refresh token presented by the client.</param>
        /// <returns>New token response when the refresh token is active; otherwise null.</returns>
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes an active refresh token during logout.
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke.</param>
        /// <returns>True when the token was found and revoked.</returns>
        Task<bool> LogoutAsync(string refreshToken);

        /// <summary>
        /// Creates a password reset token for a registered email.
        /// </summary>
        /// <param name="email">Email address requesting password reset.</param>
        /// <returns>The reset token when the user exists; otherwise null.</returns>
        Task<string?> ForgotPasswordAsync(string email);

        /// <summary>
        /// Resets the password using a valid reset token.
        /// </summary>
        /// <param name="request">Reset token and new password.</param>
        /// <returns>True when the password was reset.</returns>
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);

        /// <summary>
        /// Changes the password for an authenticated user.
        /// </summary>
        /// <param name="userId">Authenticated user identifier.</param>
        /// <param name="request">Current and new password values.</param>
        /// <returns>True when the current password matched and the change was saved.</returns>
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);

        /// <summary>
        /// Gets the authenticated user's profile.
        /// </summary>
        /// <param name="userId">Authenticated user identifier.</param>
        /// <returns>User profile when the account exists; otherwise null.</returns>
        Task<UserResponseDto?> GetProfileAsync(Guid userId);
    }
}
