using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces;
using Identity.API.Domain.Enums;
using Identity.API.Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // GET /api/users?page=1&pageSize=20&search=john&role=Admin  [Admin only]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            var users = await _userRepo.GetAllUsersAsync(page, pageSize, search, role);
            var total = await _userRepo.GetUsersCountAsync(search, role);

            return Ok(new
            {
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)total / pageSize),
                data = users
            });
        }

        // GET /api/users/{id}  [Admin or own user]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var currentUserId = User.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            // Only Admin or the user themselves can view a profile
            if (!isAdmin && currentUserId != id)
                return Forbid();

            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });
            return Ok(user);
        }

        // PUT /api/users/me  — any logged in user can update their own profile
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _userRepo.GetUserByIdAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            // Check if another user already has that email
            var emailTaken = await _userRepo.IsEmailTakenAsync(request.Email, userId.Value);
            if (emailTaken) return Conflict(new { message = "Email is already in use by another account." });

            var updatedUser = await _userRepo.UpdateProfileAsync(userId.Value, request);
            return Ok(updatedUser);
        }

        // PUT /api/users/{id}/role  [Admin only]
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> SetUserRole(Guid id, [FromBody] UpdateRoleRequestDto request)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            // Validate that the role string is a known Role enum value
            if (!Enum.TryParse<Role>(request.Role, true, out var role))
                return BadRequest(new { message = $"Invalid role. Valid values: {string.Join(", ", Enum.GetNames<Role>())}" });

            await _userRepo.UpdateUserRoleAsync(id, role);
            return Ok(new { message = $"User role updated to {role}." });
        }

        // PUT /api/users/{id}/status  [Admin only]
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] SetUserStatusRequestDto request)
        {
            var currentUserId = User.GetUserId();

            // Admin cannot deactivate themselves
            if (id == currentUserId && !request.IsActive)
                return BadRequest(new { message = "You cannot deactivate your own account." });

            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            await _userRepo.SetUserActiveAsync(id, request.IsActive);
            return Ok(new { message = $"User account {(request.IsActive ? "activated" : "deactivated")}." });
        }

        // DELETE /api/users/{id}  [Admin only]
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var currentUserId = User.GetUserId();

            // Admin cannot delete themselves
            if (id == currentUserId)
                return BadRequest(new { message = "You cannot delete your own account." });

            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            await _userRepo.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
