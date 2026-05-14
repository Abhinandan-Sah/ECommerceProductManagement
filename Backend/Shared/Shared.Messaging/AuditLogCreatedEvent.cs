namespace Shared.Messaging
{
    /// <summary>
    /// Event published when a service records a business audit action.
    /// </summary>
    public class AuditLogCreatedEvent
    {
        /// <summary>
        /// Domain entity type being audited, such as Product.
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the domain entity being audited.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Action performed on the entity, such as Created or StatusChanged.
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
