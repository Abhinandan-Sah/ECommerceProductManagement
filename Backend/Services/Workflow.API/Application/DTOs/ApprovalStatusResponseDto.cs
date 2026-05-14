using Workflow.API.Domain.Enums;

namespace Workflow.API.Application.DTOs
{
    /// <summary>
    /// Represents a product's current approval workflow state.
    /// </summary>
    public class ApprovalStatusResponseDto
    {
        /// <summary>
        /// Product identifier connected to the approval state.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Current approval status.
        /// </summary>
        public ApprovalStatus Status { get; set; }

        /// <summary>
        /// Indicates whether the product has been submitted into workflow.
        /// </summary>
        public bool IsSubmitted { get; set; }

        /// <summary>
        /// User identifier of the administrator who approved or rejected the product, when available.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Reviewer comments attached to the current approval state.
        /// </summary>
        public string? Remarks { get; set; }
    }
}
