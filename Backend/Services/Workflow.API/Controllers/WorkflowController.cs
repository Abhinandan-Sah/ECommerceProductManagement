using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflow.API.Application.DTOs;
using Workflow.API.Application.Extensions;
using Workflow.API.Application.Interfaces.Services;

namespace Workflow.API.Controllers
{
    [Route("api/workflow")]
    [ApiController]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _service;

        public WorkflowController(IWorkflowService service)
        {
            _service = service;
        }

        [HttpGet("products/{id:guid}/pricing")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> GetPricing(Guid id)
        {
            var pricing = await _service.GetPricingAsync(id);
            if (pricing == null)
            {
                return Ok(new { ProductId = id, MRP = 0, SalePrice = 0, GSTPercent = 0 });
            }
            return Ok(pricing);
        }

        [HttpPut("products/{id:guid}/pricing")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> UpdatePricing(Guid id, [FromBody] UpdatePricingRequestDto request)
        {
            await _service.UpdatePricingAsync(id, request);
            return Ok(new { message = "Pricing saved successfully." });
        }

        [HttpGet("products/{id:guid}/inventory")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> GetInventory(Guid id)
        {
            var inventory = await _service.GetInventoryAsync(id);
            if (inventory == null)
            {
                return Ok(new { ProductId = id, AvailableQty = 0, ReorderLevel = 0, WarehouseLocation = "" });
            }
            return Ok(inventory);
        }

        [HttpPut("products/{id:guid}/inventory")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> UpdateInventory(Guid id, [FromBody] UpdateInventoryRequestDto request)
        {
            await _service.UpdateInventoryAsync(id, request);
            return Ok(new { message = "Inventory saved successfully." });
        }

        [HttpPost("products/{id:guid}/submit")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<IActionResult> SubmitForReview(Guid id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            await _service.SubmitForReviewAsync(id, userId.Value);
            return Ok(new { message = "Product successfully submitted for review." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("products/{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequestDto request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            await _service.UpdateStatusAsync(id, request, userId.Value);
            return Ok(new { message = $"Product status successfully updated to {request.NewStatus}." });
        }

        [HttpGet("products/{id:guid}/status")]
        [Authorize(Roles = "Admin,ProductManager,ContentExecutive")]
        public async Task<IActionResult> GetApprovalStatus(Guid id)
        {
            var status = await _service.GetApprovalStatusAsync(id);
            if (status == null)
            {
                return Ok(new ApprovalStatusResponseDto
                {
                    ProductId = id,
                    Status = Domain.Enums.ApprovalStatus.Pending,
                    ApprovedByUserId = null,
                    Remarks = null
                });
            }
            return Ok(status);
        }

        [HttpGet("approvals/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var pending = await _service.GetPendingApprovalsAsync();
            return Ok(pending);
        }
    }
}