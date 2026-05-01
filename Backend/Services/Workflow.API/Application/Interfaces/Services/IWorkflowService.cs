using Workflow.API.Application.DTOs;
using Workflow.API.Domain.Entities;

namespace Workflow.API.Application.Interfaces.Services
{
    public interface IWorkflowService
    {
        // ─── Pricing ─────────────────────────────────────────────
        Task<Price?> GetPricingAsync(Guid productId, string? role);
        Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request, Guid actionByUserId);

        // ─── Inventory ───────────────────────────────────────────
        Task<Inventory?> GetInventoryAsync(Guid productId);
        Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request, Guid actionByUserId);

        // ─── Approval Workflow ───────────────────────────────────
        Task<bool> SubmitForReviewAsync(Guid productId, Guid submittedByUserId);
        Task<bool> UpdateStatusAsync(Guid productId, UpdateStatusRequestDto request, Guid actionByUserId);
        Task<ApprovalStatusResponseDto?> GetApprovalStatusAsync(Guid productId);
        Task<IEnumerable<ApprovalStatusResponseDto>> GetPendingApprovalsAsync();
    }
}
