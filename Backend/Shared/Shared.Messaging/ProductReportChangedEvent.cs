namespace Shared.Messaging
{
    /// <summary>
    /// Event published when a catalog product should be created, updated, or removed in reporting projections.
    /// </summary>
    public class ProductReportChangedEvent
    {
        /// <summary>
        /// Catalog product identifier represented by the reporting row.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Product display name copied from the catalog service.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Stable product stock keeping unit.
        /// </summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Current product workflow status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the user who created the product.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Category display name copied from the catalog service.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// True when reporting should remove its read-model row for the product.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
