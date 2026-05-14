using Workflow.API.Domain.Entities;

namespace Workflow.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines tracked workflow persistence operations for pricing, inventory, and approval records.
    /// </summary>
    public interface IWorkflowRepository
    {
        /// <summary>
        /// Finds the pricing row for a product.
        /// </summary>
        /// <param name="productId">Product identifier to load pricing for.</param>
        /// <returns>The tracked pricing row, or null when pricing has not been configured.</returns>
        Task<Price?> GetPricingByProductIdAsync(Guid productId);

        /// <summary>
        /// Creates or updates pricing and saves the database change immediately.
        /// </summary>
        /// <param name="price">Pricing entity to save.</param>
        Task SavePricingAsync(Price price);

        /// <summary>
        /// Finds the inventory row for a product.
        /// </summary>
        /// <param name="productId">Product identifier to load inventory for.</param>
        /// <returns>The tracked inventory row, or null when inventory has not been configured.</returns>
        Task<Inventory?> GetInventoryByProductIdAsync(Guid productId);

        /// <summary>
        /// Creates or updates inventory and saves the database change immediately.
        /// </summary>
        /// <param name="inventory">Inventory entity to save.</param>
        Task SaveInventoryAsync(Inventory inventory);

        /// <summary>
        /// Finds the current approval row for a product.
        /// </summary>
        /// <param name="productId">Product identifier to load approval state for.</param>
        /// <returns>The tracked approval row, or null when the product has not entered workflow.</returns>
        Task<Approval?> GetCurrentApprovalStatusAsync(Guid productId);

        /// <summary>
        /// Reads all approval rows waiting for review.
        /// </summary>
        /// <returns>Tracked pending approvals, or an empty collection when none are pending.</returns>
        Task<IEnumerable<Approval>> GetPendingApprovalsAsync();

        /// <summary>
        /// Creates or updates approval state and saves the database change immediately.
        /// </summary>
        /// <param name="approval">Approval entity to save.</param>
        Task SaveApprovalAsync(Approval approval);
    }
}
