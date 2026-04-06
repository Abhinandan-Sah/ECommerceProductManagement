using Workflow.API.Application.DTOs;

namespace Workflow.API.Application.Interfaces.Services
{
    public interface IWorkflowService
    {
        // ─── Pricing ─────────────────────────────────────────────
        Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request);

        // ─── Inventory ───────────────────────────────────────────
        Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request);

        // ─── Approval Workflow ───────────────────────────────────
        Task<bool> SubmitForReviewAsync(Guid productId, Guid submittedByUserId);
        Task<bool> UpdateStatusAsync(Guid productId, UpdateStatusRequestDto request, Guid actionByUserId);
    }
}
