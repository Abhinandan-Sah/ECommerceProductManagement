using Identity.API.Application.DTOs.Auth;
using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var emailExists = await _authService.EmailExistsAsync(request.Email);
            if (emailExists) return Conflict(new { message = "A user with this email already exists." });

            await _authService.RegisterAsync(request);
            return StatusCode(201, new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null) return Unauthorized(new { message = "Invalid email or password." });
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (result == null) return Unauthorized(new { message = "Invalid or expired refresh token." });
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            var success = await _authService.LogoutAsync(request.RefreshToken);
            if (!success) return BadRequest(new { message = "Invalid or already revoked token." });
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            await _authService.ForgotPasswordAsync(request.Email);

            // Always return the same generic message regardless of whether email exists
            // This prevents email enumeration attacks
            return Ok(new { message = "If this email is registered, a reset link will be sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var success = await _authService.ResetPasswordAsync(request);
            if (!success) return BadRequest(new { message = "Invalid or expired reset token." });
            return Ok(new { message = "Password reset successfully. Please log in with your new password." });
        }

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

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "Welcome, Admin!" });
        }
    }
}
