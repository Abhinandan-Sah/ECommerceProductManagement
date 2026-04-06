using Workflow.API.Domain.Entities;

namespace Workflow.API.Application.Interfaces.Repositories
{
    public interface IWorkflowRepository
    {
        Task<Price?> GetPricingByProductIdAsync(Guid productId);
        Task SavePricingAsync(Price price);

        Task<Inventory?> GetInventoryByProductIdAsync(Guid productId);
        Task SaveInventoryAsync(Inventory inventory);

        Task<Approval?> GetCurrentApprovalStatusAsync(Guid productId);
        Task SaveApprovalAsync(Approval approval);
    }
}