using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflow.API.Application.DTOs;
using Workflow.API.Application.Extensions;
using Workflow.API.Application.Interfaces.Services;
using System.Security.Claims;

namespace Workflow.API.Controllers
{
    /// <summary>
    /// Exposes pricing, inventory, and approval workflow operations for products.
    /// </summary>
    [Route("api/workflow")]
    [ApiController]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _service;

        /// <summary>
        /// Creates the workflow controller with the workflow service it delegates to.
        /// </summary>
        /// <param name="service">Service that owns workflow rules and persistence work.</param>
        public WorkflowController(IWorkflowService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets product pricing when the caller is allowed to see it.
        /// </summary>
        /// <param name="id">Product identifier to load pricing for.</param>
        /// <returns>Pricing details when visible to the caller.</returns>
        /// <response code="200">Pricing was found and returned.</response>
        /// <response code="404">Pricing does not exist or is hidden from the caller.</response>
        [HttpGet("products/{id:guid}/pricing")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPricing(Guid id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            // The service decides whether the caller can see pricing for an unapproved product.
            var pricing = await _service.GetPricingAsync(id, role);
            if (pricing == null)
            {
                return NotFound();
            }
            return Ok(pricing);
        }

        /// <summary>
        /// Creates or updates product pricing.
        /// </summary>
        /// <param name="id">Product identifier to update pricing for.</param>
        /// <param name="request">Pricing values to save.</param>
        /// <returns>A success message when pricing is saved.</returns>
        /// <response code="200">Pricing was saved successfully.</response>
        /// <response code="400">Pricing payload failed validation or violates pricing rules.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to update pricing.</response>
        [HttpPut("products/{id:guid}/pricing")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> UpdatePricing(Guid id, [FromBody] UpdatePricingRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            await _service.UpdatePricingAsync(id, request, userId.Value);
            return Ok(new { message = "Pricing saved successfully." });
        }

        /// <summary>
        /// Gets product inventory for product management users.
        /// </summary>
        /// <param name="id">Product identifier to load inventory for.</param>
        /// <returns>Inventory details, or an empty setup shape when inventory has not been created.</returns>
        /// <response code="200">Inventory data or setup defaults were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view inventory.</response>
        [HttpGet("products/{id:guid}/inventory")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> GetInventory(Guid id)
        {
            var inventory = await _service.GetInventoryAsync(id);
            if (inventory == null)
            {
                // Return an empty shape for first-time setup so the UI can bind the editor without special cases.
                return Ok(new { ProductId = id, AvailableQty = 0, ReorderLevel = 0, WarehouseLocation = "" });
            }
            return Ok(inventory);
        }

        /// <summary>
        /// Creates or updates product inventory.
        /// </summary>
        /// <param name="id">Product identifier to update inventory for.</param>
        /// <param name="request">Inventory values to save.</param>
        /// <returns>A success message when inventory is saved.</returns>
        /// <response code="200">Inventory was saved successfully.</response>
        /// <response code="400">Inventory payload failed validation.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to update inventory.</response>
        [HttpPut("products/{id:guid}/inventory")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> UpdateInventory(Guid id, [FromBody] UpdateInventoryRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            await _service.UpdateInventoryAsync(id, request, userId.Value);
            return Ok(new { message = "Inventory saved successfully." });
        }

        /// <summary>
        /// Submits a product into the approval workflow.
        /// </summary>
        /// <param name="id">Product identifier to submit.</param>
        /// <returns>A success message when the product is submitted.</returns>
        /// <response code="200">Product was submitted for review successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to submit products.</response>
        [HttpPost("products/{id:guid}/submit")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<IActionResult> SubmitForReview(Guid id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            await _service.SubmitForReviewAsync(id, userId.Value);
            return Ok(new { message = "Product successfully submitted for review." });
        }

        /// <summary>
        /// Updates a product's approval status.
        /// </summary>
        /// <param name="id">Product identifier whose approval status should change.</param>
        /// <param name="request">New approval status and optional remarks.</param>
        /// <returns>A success message when the status is updated.</returns>
        /// <response code="200">Approval status was updated successfully.</response>
        /// <response code="400">Product has not been submitted or payload validation failed.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("products/{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            await _service.UpdateStatusAsync(id, request, userId.Value);
            return Ok(new { message = $"Product status successfully updated to {request.NewStatus}." });
        }

        /// <summary>
        /// Gets the current approval status for a product.
        /// </summary>
        /// <param name="id">Product identifier to load approval status for.</param>
        /// <returns>The current approval status, or a not-submitted status shape.</returns>
        /// <response code="200">Approval status was returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller does not have permission to view approval status.</response>
        [HttpGet("products/{id:guid}/status")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<IActionResult> GetApprovalStatus(Guid id)
        {
            var status = await _service.GetApprovalStatusAsync(id);
            if (status == null)
            {
                // No workflow row yet means the product has not been submitted, not that the request failed.
                return Ok(new ApprovalStatusResponseDto
                {
                    ProductId = id,
                    Status = Domain.Enums.ApprovalStatus.Pending,
                    IsSubmitted = false,
                    ApprovedByUserId = null,
                    Remarks = null
                });
            }
            return Ok(status);
        }

        /// <summary>
        /// Gets all products currently waiting for approval.
        /// </summary>
        /// <returns>Pending approval records.</returns>
        /// <response code="200">Pending approvals were returned successfully.</response>
        /// <response code="401">Caller is not authenticated.</response>
        /// <response code="403">Caller is not an administrator.</response>
        [HttpGet("approvals/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var pending = await _service.GetPendingApprovalsAsync();
            return Ok(pending);
        }
    }
}
