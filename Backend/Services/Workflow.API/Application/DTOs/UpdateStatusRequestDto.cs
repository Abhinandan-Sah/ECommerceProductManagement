using System.ComponentModel.DataAnnotations;
using Workflow.API.Domain.Enums;

namespace Workflow.API.Application.DTOs
{
    /// <summary>
    /// Captures an approval status change requested by an administrator.
    /// </summary>
    public class UpdateStatusRequestDto
    {
        /// <summary>
        /// Approval status to apply to the product.
        /// </summary>
        [Required(ErrorMessage = "New status is required")]
        public ApprovalStatus NewStatus { get; set; }

        // Remarks are optional for approval, but typically required for rejection
        /// <summary>
        /// Optional reviewer comments explaining the approval decision.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }
    }
}
