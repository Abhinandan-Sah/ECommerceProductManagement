using Workflow.API.Domain.Enums;

namespace Workflow.API.Domain.Entities
{
    /// <summary>
    /// Represents the approval workflow state for a product.
    /// </summary>
    public class Approval : BaseEntity
    {
        /// <summary>
        /// Product identifier connected to this approval row.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Current approval status.
        /// </summary>
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        /// <summary>
        /// User identifier of the person who submitted the product for review.
        /// </summary>
        public Guid SubmittedByUserId { get; set; }

        /// <summary>
        /// User identifier of the administrator who approved or rejected the product, when available.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Reviewer comments attached to the approval decision.
        /// </summary>
        public string? Remarks { get; set; }
    }
}
