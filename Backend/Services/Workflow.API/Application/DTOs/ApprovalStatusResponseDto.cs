using Workflow.API.Domain.Enums;

namespace Workflow.API.Application.DTOs
{
    public class ApprovalStatusResponseDto
    {
        public Guid ProductId { get; set; }
        public ApprovalStatus Status { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? Remarks { get; set; }
    }
}
