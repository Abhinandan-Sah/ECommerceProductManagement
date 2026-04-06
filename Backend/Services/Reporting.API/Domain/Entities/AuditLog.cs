namespace Reporting.API.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // e.g., "Product", "Price", "Inventory", "Approval"
        public string EntityName { get; set; } = string.Empty;

        // The ID of the specific product or price that was changed
        public Guid EntityId { get; set; }

        // e.g., "Created", "Updated", "Approved", "Published"
        public string Action { get; set; } = string.Empty;

        public Guid ByUserId { get; set; }
        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        // Enterprise Standard: Store the "Before" and "After" state as a JSON string
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}