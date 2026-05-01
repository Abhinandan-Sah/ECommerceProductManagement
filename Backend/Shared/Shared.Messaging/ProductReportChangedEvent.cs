namespace Shared.Messaging
{
    public class ProductReportChangedEvent
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}
