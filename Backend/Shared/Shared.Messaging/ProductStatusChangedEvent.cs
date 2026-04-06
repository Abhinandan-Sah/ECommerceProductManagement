namespace Shared.Messaging
{
    // This is the exact envelope that will travel through RabbitMQ
    public class ProductStatusChangedEvent
    {
        public Guid ProductId { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public Guid ChangedByUserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}