using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    /// <summary>
    /// Handles sign-in, registration, password recovery, and the current user's profile.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Creates the authentication controller with the account service it uses for security work.
        /// </summary>
        /// <param name="authService">Service that owns authentication and account-security behavior.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new customer account when the email address is not already in use.
        /// </summary>
        /// <param name="request">The new user's name, email, and password.</param>
        /// <returns>A created response when registration succeeds, or a conflict when the email already exists.</returns>
        /// <response code="201">User account was registered successfully.</response>
        /// <response code="400">Registration details failed validation.</response>
        /// <response code="409">A user with the email already exists.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var emailExists = await _authService.EmailExistsAsync(request.Email);
            if (emailExists) return Conflict(new { message = "A user with this email already exists." });

            await _authService.RegisterAsync(request);
            return StatusCode(201, new { message = "User registered successfully." });
        }

        /// <summary>
        /// Signs in a user and returns tokens when the supplied credentials are valid.
        /// </summary>
        /// <param name="request">The email and password submitted by the user.</param>
        /// <returns>An authentication response, or unauthorized when the login cannot be trusted.</returns>
        /// <response code="200">Credentials were valid and tokens were issued.</response>
        /// <response code="400">Login details failed validation.</response>
        /// <response code="401">Email or password is invalid, or the account is inactive.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null) return Unauthorized(new { message = "Invalid email or password." });
            return Ok(result);
        }

        /// <summary>
        /// Exchanges an active refresh token for a fresh access token and refresh token.
        /// </summary>
        /// <param name="request">The refresh token currently held by the client.</param>
        /// <returns>A new token pair, or unauthorized when the refresh token is invalid or expired.</returns>
        /// <response code="200">Refresh token was accepted and a new token pair was issued.</response>
        /// <response code="400">Refresh token payload failed validation.</response>
        /// <response code="401">Refresh token is invalid, expired, revoked, or belongs to an inactive user.</response>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (result == null) return Unauthorized(new { message = "Invalid or expired refresh token." });
            return Ok(result);
        }

        /// <summary>
        /// Revokes the supplied refresh token so the current session can no longer be renewed.
        /// </summary>
        /// <param name="request">The refresh token to revoke.</param>
        /// <returns>A success message when logout is completed, or a bad request for an unusable token.</returns>
        /// <response code="200">Refresh token was revoked successfully.</response>
        /// <response code="400">Refresh token is invalid or already revoked.</response>
        /// <response code="401">Caller is not authenticated.</response>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            var success = await _authService.LogoutAsync(request.RefreshToken);
            if (!success) return BadRequest(new { message = "Invalid or already revoked token." });
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Starts the password reset flow without revealing whether the email belongs to an account.
        /// </summary>
        /// <param name="request">The email address that should receive password reset instructions.</param>
        /// <returns>The same accepted response for known and unknown email addresses.</returns>
        /// <response code="200">Password reset flow was accepted without revealing account existence.</response>
        /// <response code="400">Email payload failed validation.</response>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            await _authService.ForgotPasswordAsync(request.Email);

            // Same response for known and unknown emails; the caller should not be able to probe accounts.
            return Ok(new { message = "If this email is registered, a reset link will be sent." });
        }

        /// <summary>
        /// Replaces a user's password after a valid reset token is presented.
        /// </summary>
        /// <param name="request">The reset token and the new password chosen by the user.</param>
        /// <returns>A success message, or a bad request when the reset token cannot be used.</returns>
        /// <response code="200">Password was reset successfully.</response>
        /// <response code="400">Reset token is invalid, expired, used, or the password payload failed validation.</response>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var success = await _authService.ResetPasswordAsync(request);
            if (!success) return BadRequest(new { message = "Invalid or expired reset token." });
            return Ok(new { message = "Password reset successfully. Please log in with your new password." });
        }

        /// <summary>
        /// Lets an authenticated user change their password after proving the current password.
        /// </summary>
        /// <param name="request">The user's current password and the replacement password.</param>
        /// <returns>A success message, or a bad request when the current password is wrong.</returns>
        /// <response code="200">Password was changed successfully.</response>
        /// <response code="400">Current password is incorrect or the password payload failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var success = await _authService.ChangePasswordAsync(userId.Value, request);
            if (!success) return BadRequest(new { message = "Current password is incorrect." });
            return Ok(new { message = "Password changed successfully. Please log in again." });
        }

        /// <summary>
        /// Gets the profile for the authenticated user.
        /// </summary>
        /// <returns>The user's profile, or not found if the account no longer exists.</returns>
        /// <response code="200">Profile was found and returned.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="404">Authenticated user no longer exists.</response>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authService.GetProfileAsync(userId.Value);
            if (profile == null) return NotFound(new { message = "User not found." });
            return Ok(profile);
        }

        /// <summary>
        /// Confirms that the current user has administrator access.
        /// </summary>
        /// <returns>A small administrator-only response.</returns>
        /// <response code="200">Caller has administrator access.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "Welcome, Admin!" });
        }
    }
}
