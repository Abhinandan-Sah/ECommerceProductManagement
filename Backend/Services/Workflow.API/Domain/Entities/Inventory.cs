namespace Workflow.API.Domain.Entities
{
    /// <summary>
    /// Represents inventory details for a product.
    /// </summary>
    public class Inventory : BaseEntity
    {
        /// <summary>
        /// Product identifier connected to this inventory row.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Warehouse or storage location for the product stock.
        /// </summary>
        public string WarehouseLocation { get; set; } = string.Empty;

        /// <summary>
        /// Quantity currently available for sale or allocation.
        /// </summary>
        public int AvailableQty { get; set; }

        /// <summary>
        /// Quantity reserved for pending orders or operations.
        /// </summary>
        public int ReservedQty { get; set; }

        /// <summary>
        /// Stock level at which the product should be reordered.
        /// </summary>
        public int ReorderLevel { get; set; }
    }
}
