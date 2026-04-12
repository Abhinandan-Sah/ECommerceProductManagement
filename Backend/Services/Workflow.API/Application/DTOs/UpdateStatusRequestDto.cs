using System.ComponentModel.DataAnnotations;
using Workflow.API.Domain.Enums;

namespace Workflow.API.Application.DTOs
{
    public class UpdateStatusRequestDto
    {
        [Required(ErrorMessage = "New status is required")]
        public ApprovalStatus NewStatus { get; set; }

        // Remarks are optional for approval, but typically required for rejection
        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }
    }
}