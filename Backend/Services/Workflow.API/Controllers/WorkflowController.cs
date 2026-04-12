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

        [HttpPut("products/{id:guid}/pricing")]
        [Authorize(Roles = "Admin,ProductManager")]
        public async Task<IActionResult> UpdatePricing(Guid id, [FromBody] UpdatePricingRequestDto request)
        {
            await _service.UpdatePricingAsync(id, request);
            return Ok(new { message = "Pricing saved successfully." });
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
    }
}