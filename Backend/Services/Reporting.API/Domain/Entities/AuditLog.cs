namespace Reporting.API.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid ByUserId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}