namespace Workflow.API.Domain.Entities
{
    public class Price : BaseEntity
    {
        public Guid ProductId { get; set; }
        public decimal MRP { get; set; }
        public decimal SalePrice { get; set; }
        public decimal GSTPercent { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    }
}