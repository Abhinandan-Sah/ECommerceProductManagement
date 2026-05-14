namespace Reporting.API.Domain.Entities
{
    /// <summary>
    /// Represents an audit event persisted by the reporting service.
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// Unique audit log identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Domain entity type being tracked, such as Product.
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the domain entity being tracked.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Action performed on the entity, such as StatusChanged.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the user who performed the action.
        /// </summary>
        public Guid ByUserId { get; set; }

        /// <summary>
        /// Serialized JSON snapshot before the change, when available.
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// Serialized JSON snapshot after the change, when available.
        /// </summary>
        public string? NewValues { get; set; }
    }
}
