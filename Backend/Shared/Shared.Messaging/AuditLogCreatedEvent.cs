namespace Shared.Messaging
{
    /// <summary>
    /// Generic audit event for user actions that should be persisted by Reporting.API.
    /// </summary>
    public class AuditLogCreatedEvent
    {
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid ByUserId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
