namespace Workflow.API.Domain.Entities
{
    /// <summary>
    /// Represents the pricing configuration for a product.
    /// </summary>
    public class Price : BaseEntity
    {
        /// <summary>
        /// Product identifier connected to this pricing row.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Maximum retail price for the product.
        /// </summary>
        public decimal MRP { get; set; }

        /// <summary>
        /// Sale price charged to customers.
        /// </summary>
        public decimal SalePrice { get; set; }

        /// <summary>
        /// GST percentage applied to the product price.
        /// </summary>
        public decimal GSTPercent { get; set; }

        /// <summary>
        /// UTC time when this pricing becomes effective.
        /// </summary>
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    }
}
