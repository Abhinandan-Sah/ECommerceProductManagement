using System.ComponentModel.DataAnnotations;
using Workflow.API.Domain.Enums;

namespace Workflow.API.Application.DTOs
{
    public class UpdateStatusRequestDto
    {
        [Required]
        public ApprovalStatus NewStatus { get; set; }

        // Remarks are optional for approval, but typically required for rejection
        public string? Remarks { get; set; }
    }
}