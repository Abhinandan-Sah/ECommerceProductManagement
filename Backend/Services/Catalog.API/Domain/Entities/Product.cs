using Catalog.API.Domain.Enums;

namespace Catalog.API.Domain.Entities
{
    /// <summary>
    /// Represents a catalog product and its publishing workflow state.
    /// </summary>
    public class Product : BaseEntity
    {
        /// <summary>
        /// Unique product identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Product name shown in catalog views.
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Brand that owns or manufactures the product.
        /// </summary>
        public string Brand { get; set; } = String.Empty;

        /// <summary>
        /// Customer-facing product description.
        /// </summary>
        public string Description { get; set; } = String.Empty;

        /// <summary>
        /// Stable product stock keeping unit.
        /// </summary>
        public string SKU { get; set; } = String.Empty;

        /// <summary>
        /// Current workflow state for publishing the product.
        /// </summary>
        public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;

        /// <summary>
        /// Category identifier the product belongs to.
        /// </summary>
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Category navigation property.
        /// </summary>
        public Category? Category { get; set; }

        /// <summary>
        /// Variant records available for this product.
        /// </summary>
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        /// <summary>
        /// Media assets attached to this product.
        /// </summary>
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();


    }
}
