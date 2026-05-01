using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Domain.Enums;
using Identity.API.Application.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging;
using System.Text.Json;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPublishEndpoint _publishEndpoint;

        public UsersController(IUserService userService, IPublishEndpoint publishEndpoint)
        {
            _userService = userService;
            _publishEndpoint = publishEndpoint;
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
            var users = await _userService.GetAllUsersAsync(page, pageSize, search, role);
            var total = await _userService.GetUsersCountAsync(search, role);

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

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });
            return Ok(user);
        }

        // PUT /api/users/me  — any logged in user can update their own profile
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            // Check if another user already has that email
            var emailTaken = await _userService.IsEmailTakenAsync(request.Email, userId.Value);
            if (emailTaken) return Conflict(new { message = "Email is already in use by another account." });

            var updatedUser = await _userService.UpdateProfileAsync(userId.Value, request);
            await PublishAuditLogAsync(
                "User",
                userId.Value,
                "ProfileUpdated",
                userId.Value,
                JsonSerializer.Serialize(new { user.FullName, user.Email }),
                JsonSerializer.Serialize(new { updatedUser.FullName, updatedUser.Email }));

            return Ok(updatedUser);
        }

        // PUT /api/users/{id}/role  [Admin only]
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> SetUserRole(Guid id, [FromBody] UpdateRoleRequestDto request)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            // Validate that the role string is a known Role enum value
            if (!Enum.TryParse<Role>(request.Role, true, out var role))
                return BadRequest(new { message = $"Invalid role. Valid values: {string.Join(", ", Enum.GetNames<Role>())}" });

            var adminUserId = User.GetUserId();
            if (adminUserId == null) return Unauthorized();

            await _userService.UpdateUserRoleAsync(id, role);
            await PublishAuditLogAsync(
                "User",
                id,
                "RoleChanged",
                adminUserId.Value,
                JsonSerializer.Serialize(new { user.Role }),
                JsonSerializer.Serialize(new { Role = role.ToString() }));

            return Ok(new { message = $"User role updated to {role}." });
        }

        // PUT /api/users/{id}/status  [Admin only]
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] SetUserStatusRequestDto request)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == null) return Unauthorized();

            // Admin cannot deactivate themselves
            if (id == currentUserId && !request.IsActive)
                return BadRequest(new { message = "You cannot deactivate your own account." });

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            await _userService.SetUserActiveAsync(id, request.IsActive);
            await PublishAuditLogAsync(
                "User",
                id,
                "StatusChanged",
                currentUserId.Value,
                JsonSerializer.Serialize(new { user.IsActive }),
                JsonSerializer.Serialize(new { request.IsActive }));

            return Ok(new { message = $"User account {(request.IsActive ? "activated" : "deactivated")}." });
        }

        // DELETE /api/users/{id}  [Admin only]
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == null) return Unauthorized();

            // Admin cannot delete themselves
            if (id == currentUserId)
                return BadRequest(new { message = "You cannot delete your own account." });

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            await _userService.DeleteUserAsync(id);
            await _publishEndpoint.Publish(new UserCountChangedEvent { Delta = -1 });
            await PublishAuditLogAsync(
                "User",
                id,
                "Deleted",
                currentUserId.Value,
                JsonSerializer.Serialize(new { user.FullName, user.Email, user.Role, user.IsActive }),
                null);

            return Ok(new { message = "User deleted successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var stats = await _userService.GetUserStatsAsync();
            return Ok(stats);
        }

        private Task PublishAuditLogAsync(
            string entityName,
            Guid entityId,
            string action,
            Guid byUserId,
            string? oldValues,
            string? newValues)
        {
            return _publishEndpoint.Publish(new AuditLogCreatedEvent
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                ByUserId = byUserId,
                OldValues = oldValues,
                NewValues = newValues
            });
        }
    }
}
