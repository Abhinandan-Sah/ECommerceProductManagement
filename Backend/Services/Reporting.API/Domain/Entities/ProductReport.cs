using System;

namespace Reporting.API.Domain.Entities
{
    /// <summary>
    /// Represents the reporting read model for a catalog product.
    /// </summary>
    public class ProductReport : BaseEntity
    {
        /// <summary>
        /// Unique product report row identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Catalog product identifier represented by this report row.
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
        /// UTC time when the product was first published, when available.
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Identifier of the user who created the product.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Category display name copied from the catalog service.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
    }
}
