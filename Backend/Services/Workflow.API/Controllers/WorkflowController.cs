using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workflow.API.Application.DTOs;
using Workflow.API.Application.Interfaces.Services;

namespace Workflow.API.Controllers
{
    [Route("api/workflow")]
    [ApiController]
    [Authorize] // Enforces that EVERY endpoint requires a valid JWT token
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _service;

        public WorkflowController(IWorkflowService service)
        {
            _service = service;
        }

        // Helper method to extract the UserId from the JWT Token
        private Guid GetUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }

        // ─── PRICING ────────────────────────────────────────────────────────
        // PUT /api/workflow/products/{id}/pricing
        [HttpPut("products/{id:guid}/pricing")]
        public async Task<IActionResult> UpdatePricing(Guid id, [FromBody] UpdatePricingRequestDto request)
        {
            var success = await _service.UpdatePricingAsync(id, request);
            if (!success) return BadRequest(new { message = "Failed to update pricing." });

            return Ok(new { message = "Pricing saved successfully." });
        }

        // ─── INVENTORY ──────────────────────────────────────────────────────
        // PUT /api/workflow/products/{id}/inventory
        [HttpPut("products/{id:guid}/inventory")]
        public async Task<IActionResult> UpdateInventory(Guid id, [FromBody] UpdateInventoryRequestDto request)
        {
            var success = await _service.UpdateInventoryAsync(id, request);
            if (!success) return BadRequest(new { message = "Failed to update inventory." });

            return Ok(new { message = "Inventory saved successfully." });
        }

        // ─── WORKFLOW SUBMISSION ────────────────────────────────────────────
        // POST /api/workflow/products/{id}/submit
        [HttpPost("products/{id:guid}/submit")]
        public async Task<IActionResult> SubmitForReview(Guid id)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var success = await _service.SubmitForReviewAsync(id, userId);
            if (!success) return BadRequest(new { message = "Failed to submit product for review." });

            return Ok(new { message = "Product successfully submitted for review." });
        }

        // ─── WORKFLOW STATUS (ADMIN ONLY) ───────────────────────────────────
        // PUT /api/workflow/products/{id}/status
        [Authorize(Roles = "Admin")] // Critical: Only Admins can approve/publish!
        [HttpPut("products/{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequestDto request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var success = await _service.UpdateStatusAsync(id, request, userId);
            if (!success) return BadRequest(new { message = "Product must be submitted before it can be approved/rejected." });

            return Ok(new { message = $"Product status successfully updated to {request.NewStatus}." });
        }
    }
}