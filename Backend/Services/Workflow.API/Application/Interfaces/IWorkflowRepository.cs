using Workflow.API.Application.DTOs;
using Workflow.API.Domain.Entities;
using Workflow.API.Domain.Enums;

namespace Workflow.API.Application.Interfaces
{
    public interface IWorkflowRepository
    {
        // ─── Pricing ─────────────────────────────────────────────
        Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request);
        Task<Price?> GetPricingByProductIdAsync(Guid productId);

        // ─── Inventory ───────────────────────────────────────────
        Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request);
        Task<Inventory?> GetInventoryByProductIdAsync(Guid productId);

        // ─── Approval Workflow ───────────────────────────────────
        Task<bool> SubmitForReviewAsync(Guid productId, Guid submittedByUserId);
        Task<bool> UpdateStatusAsync(Guid productId, UpdateStatusRequestDto request, Guid actionByUserId);
        Task<Approval?> GetCurrentApprovalStatusAsync(Guid productId);
    }
}