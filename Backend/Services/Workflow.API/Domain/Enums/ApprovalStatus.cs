namespace Workflow.API.Domain.Enums
{
    public enum ApprovalStatus
    {
        Draft,
        InEnrichment,
        ReadyForReview,
        Approved,
        Published,
        Rejected,
        Archived
    }
}