using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces;
using Identity.API.Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;

        public AuthController(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var emailExists = await _authRepo.EmailExistsAsync(request.Email);
            if (emailExists) return Conflict(new { message = "A user with this email already exists." });

            await _authRepo.RegisterAsync(request);
            return StatusCode(201, new { message = "User registered successfully." });
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authRepo.LoginAsync(request);
            if (result == null) return Unauthorized(new { message = "Invalid email or password." });
            return Ok(result);
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authRepo.RefreshTokenAsync(request.RefreshToken);
            if (result == null) return Unauthorized(new { message = "Invalid or expired refresh token." });
            return Ok(result);
        }

        // POST /api/auth/logout  [Authorize]
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            var success = await _authRepo.LogoutAsync(request.RefreshToken);
            if (!success) return BadRequest(new { message = "Invalid or already revoked token." });
            return Ok(new { message = "Logged out successfully." });
        }

        // POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var resetToken = await _authRepo.ForgotPasswordAsync(request.Email);

            // Always return 200 — prevents attackers from knowing which emails are registered
            if (resetToken == null)
                return Ok(new { message = "If this email is registered, a reset token will be sent." });

            // TODO: Email this token to the user in production. Returning it here for development only.
            return Ok(new { message = "Reset token generated.", resetToken });
        }

        // POST /api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var success = await _authRepo.ResetPasswordAsync(request);
            if (!success) return BadRequest(new { message = "Invalid or expired reset token." });
            return Ok(new { message = "Password reset successfully. Please log in with your new password." });
        }

        // POST /api/auth/change-password  [Authorize]
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var success = await _authRepo.ChangePasswordAsync(userId.Value, request);
            if (!success) return BadRequest(new { message = "Current password is incorrect." });
            return Ok(new { message = "Password changed successfully. Please log in again." });
        }

        // GET /api/auth/profile  [Authorize]
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authRepo.GetProfileAsync(userId.Value);
            if (profile == null) return NotFound(new { message = "User not found." });
            return Ok(profile);
        }

        // GET /api/auth/admin  [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "Welcome, Admin!" });
        }
    }
}
