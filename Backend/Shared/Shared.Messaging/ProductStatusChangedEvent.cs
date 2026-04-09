namespace Shared.Messaging
{
    // Shared event contract used by Workflow (publisher) and Reporting (consumer).
    public class ProductStatusChangedEvent
    {
        // Product whose status changed.
        public Guid ProductId { get; set; }

        // Status before update.
        public string OldStatus { get; set; } = string.Empty;

        // Status after update.
        public string NewStatus { get; set; } = string.Empty;

        // User who performed the status update.
        public Guid ChangedByUserId { get; set; }
    }
}