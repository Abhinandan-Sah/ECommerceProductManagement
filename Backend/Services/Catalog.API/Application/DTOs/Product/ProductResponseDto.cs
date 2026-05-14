using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.DTOs.Product
{
    /// <summary>
    /// Represents product data returned by the API.
    /// </summary>
    public class ProductResponseDto
    {
        /// <summary>
        /// Unique product identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product name shown in catalog views.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Stable product stock keeping unit.
        /// </summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Brand that owns or manufactures the product.
        /// </summary>
        public string Brand { get; set; } = string.Empty;

        /// <summary>
        /// Customer-facing product description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Current workflow state for publishing the product.
        /// </summary>
        public PublishStatus PublishStatus { get; set; }

        /// <summary>
        /// Category identifier the product belongs to.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Category display name shown with the product.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
    }
}
