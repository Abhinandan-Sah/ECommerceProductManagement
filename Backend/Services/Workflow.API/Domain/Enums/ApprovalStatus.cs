namespace Workflow.API.Domain.Enums
{
    /// <summary>
    /// Describes where a product sits in the approval workflow.
    /// </summary>
    public enum ApprovalStatus
    {
        /// <summary>
        /// Product is waiting for review.
        /// </summary>
        Pending,

        /// <summary>
        /// Product has passed review.
        /// </summary>
        Approved,

        /// <summary>
        /// Product did not pass review and needs changes.
        /// </summary>
        Rejected
    }
}
