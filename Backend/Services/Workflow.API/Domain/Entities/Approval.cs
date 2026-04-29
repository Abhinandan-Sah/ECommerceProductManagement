using Workflow.API.Domain.Enums;

namespace Workflow.API.Domain.Entities
{
    public class Approval : BaseEntity
    {
        public Guid ProductId { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public Guid SubmittedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? Remarks { get; set; }
    }
}