using System;

namespace Reporting.API.Domain.Entities
{
    public class ProductReport : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
