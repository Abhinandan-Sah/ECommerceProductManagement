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
    /// <summary>
    /// Manages user profiles, administrator account controls, and user-related audit events.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Creates the users controller with user-management and messaging dependencies.
        /// </summary>
        /// <param name="userService">Service that owns user profile and account-management rules.</param>
        /// <param name="publishEndpoint">Message publisher used to notify other services about user changes.</param>
        public UsersController(IUserService userService, IPublishEndpoint publishEndpoint)
        {
            _userService = userService;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Lists users for administrators, with optional paging, search, and role filters.
        /// </summary>
        /// <param name="page">Page number to return.</param>
        /// <param name="pageSize">Number of users to include on each page.</param>
        /// <param name="search">Optional name or email text to search for.</param>
        /// <param name="role">Optional role name to filter by.</param>
        /// <returns>A paged list of users and the total number of matching accounts.</returns>
        /// <response code="200">Users were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
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

        /// <summary>
        /// Gets one user's profile when the caller is an administrator or the same user.
        /// </summary>
        /// <param name="id">Identifier of the user profile to read.</param>
        /// <returns>The requested user profile, or an authorization/not-found response.</returns>
        /// <response code="200">User profile was found and returned.</response>
        /// <response code="403">Caller is neither an administrator nor the requested user.</response>
        /// <response code="404">No user exists for the supplied identifier.</response>
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

        /// <summary>
        /// Updates the authenticated user's own profile details.
        /// </summary>
        /// <param name="request">The new profile values supplied by the user.</param>
        /// <returns>The updated profile, or a validation response when the request cannot be applied.</returns>
        /// <response code="200">Profile was updated successfully.</response>
        /// <response code="400">Profile payload failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="404">Authenticated user no longer exists.</response>
        /// <response code="409">Email is already used by another account.</response>
        // PUT /api/users/me - any logged-in user can update their own profile
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

        /// <summary>
        /// Changes a user's role after an administrator chooses a valid role value.
        /// </summary>
        /// <param name="id">Identifier of the user whose role should change.</param>
        /// <param name="request">Role requested by the administrator.</param>
        /// <returns>A confirmation message, or a validation response when the user or role is invalid.</returns>
        /// <response code="200">User role was updated successfully.</response>
        /// <response code="400">Requested role is not valid.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
        /// <response code="404">No user exists for the supplied identifier.</response>
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

        /// <summary>
        /// Activates or deactivates a user account, while protecting an administrator from disabling their own account.
        /// </summary>
        /// <param name="id">Identifier of the user whose status should change.</param>
        /// <param name="request">Requested active state for the account.</param>
        /// <returns>A confirmation message, or a validation response when the change is not allowed.</returns>
        /// <response code="200">User status was updated successfully.</response>
        /// <response code="400">Administrator attempted to deactivate their own account.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
        /// <response code="404">No user exists for the supplied identifier.</response>
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

        /// <summary>
        /// Deletes a user account after administrator checks have passed.
        /// </summary>
        /// <param name="id">Identifier of the user to delete.</param>
        /// <returns>A confirmation message, or a validation response when the deletion is not allowed.</returns>
        /// <response code="200">User was deleted successfully.</response>
        /// <response code="400">Administrator attempted to delete their own account.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
        /// <response code="404">No user exists for the supplied identifier.</response>
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

        /// <summary>
        /// Gets a small role breakdown that administrators can use for dashboard counts.
        /// </summary>
        /// <returns>User counts grouped by role.</returns>
        /// <response code="200">User role statistics were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var stats = await _userService.GetUserStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Publishes an audit event that records who changed a user and what values moved.
        /// </summary>
        /// <param name="entityName">Name of the entity being audited.</param>
        /// <param name="entityId">Identifier of the entity being audited.</param>
        /// <param name="action">Action that was performed.</param>
        /// <param name="byUserId">Identifier of the user who performed the action.</param>
        /// <param name="oldValues">Serialized values before the change, when available.</param>
        /// <param name="newValues">Serialized values after the change, when available.</param>
        /// <returns>A task that completes when the audit event has been published.</returns>
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
