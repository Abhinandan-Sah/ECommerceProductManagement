namespace Shared.Messaging
{
    /// <summary>
    /// Event published when Workflow changes a product's publishing status.
    /// </summary>
    public class ProductStatusChangedEvent
    {
        /// <summary>
        /// Unique identifier of the product whose status changed.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Workflow status before the change was applied.
        /// </summary>
        public string OldStatus { get; set; } = string.Empty;

        /// <summary>
        /// Workflow status after the change was applied.
        /// </summary>
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the user who performed the status change.
        /// </summary>
        public Guid ChangedByUserId { get; set; }
    }
}
