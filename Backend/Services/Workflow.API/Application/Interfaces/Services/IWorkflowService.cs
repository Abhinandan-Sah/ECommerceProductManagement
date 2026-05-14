using Workflow.API.Application.DTOs;
using Workflow.API.Domain.Entities;

namespace Workflow.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines workflow operations for pricing, inventory, and approval state.
    /// </summary>
    public interface IWorkflowService
    {
        // Pricing

        /// <summary>
        /// Gets pricing for a product when the caller is allowed to see it.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <param name="role">Role of the caller, if authenticated.</param>
        /// <returns>Pricing details when visible; otherwise null.</returns>
        Task<Price?> GetPricingAsync(Guid productId, string? role);

        /// <summary>
        /// Creates or updates product pricing and writes an audit event.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <param name="request">Pricing values to save.</param>
        /// <param name="actionByUserId">User performing the change.</param>
        /// <returns>True when pricing is saved.</returns>
        Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request, Guid actionByUserId);

        // Inventory

        /// <summary>
        /// Gets inventory for a product.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <returns>Inventory details when they exist; otherwise null.</returns>
        Task<Inventory?> GetInventoryAsync(Guid productId);

        /// <summary>
        /// Creates or updates product inventory and writes an audit event.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <param name="request">Inventory values to save.</param>
        /// <param name="actionByUserId">User performing the change.</param>
        /// <returns>True when inventory is saved.</returns>
        Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request, Guid actionByUserId);

        // Approval workflow

        /// <summary>
        /// Submits a product into the approval workflow.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <param name="submittedByUserId">User submitting the product.</param>
        /// <returns>True when the submission state is saved.</returns>
        Task<bool> SubmitForReviewAsync(Guid productId, Guid submittedByUserId);

        /// <summary>
        /// Updates approval status and publishes the status-change event.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <param name="request">New approval status and remarks.</param>
        /// <param name="actionByUserId">User performing the approval action.</param>
        /// <returns>True when the status is saved.</returns>
        Task<bool> UpdateStatusAsync(Guid productId, UpdateStatusRequestDto request, Guid actionByUserId);

        /// <summary>
        /// Gets the current approval status for a product.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <returns>Approval status when the product has entered workflow; otherwise null.</returns>
        Task<ApprovalStatusResponseDto?> GetApprovalStatusAsync(Guid productId);

        /// <summary>
        /// Gets all products waiting for approval.
        /// </summary>
        /// <returns>Pending approval records.</returns>
        Task<IEnumerable<ApprovalStatusResponseDto>> GetPendingApprovalsAsync();
    }
}
