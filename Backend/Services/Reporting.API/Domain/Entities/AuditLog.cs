namespace Reporting.API.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        // Unique row id for this audit entry.
        public Guid Id { get; set; } = Guid.NewGuid();

        // Domain entity being tracked (example: Product).
        public string EntityName { get; set; } = string.Empty;

        // Id of the domain entity being tracked.
        public Guid EntityId { get; set; }

        // Action performed on the entity (example: StatusChanged).
        public string Action { get; set; } = string.Empty;

        // User who performed the action.
        public Guid ByUserId { get; set; }

        // JSON snapshot before the change.
        public string? OldValues { get; set; }

        // JSON snapshot after the change.
        public string? NewValues { get; set; }
    }
}