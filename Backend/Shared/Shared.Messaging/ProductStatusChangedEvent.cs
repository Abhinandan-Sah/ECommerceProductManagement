namespace Shared.Messaging
{
    /// <summary>
    /// Event published when a product's workflow status changes in the system.
    /// Used for audit logging and cross-service notifications.
    /// </summary>
    public class ProductStatusChangedEvent
    {
        /// <summary>
        /// The unique identifier of the product whose status changed.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// The workflow status before the change was applied.
        /// </summary>
        public string OldStatus { get; set; } = string.Empty;

        /// <summary>
        /// The workflow status after the change was applied.
        /// </summary>
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the user who performed the status change.
        /// </summary>
        public Guid ChangedByUserId { get; set; }
    }
}