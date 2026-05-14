using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.Product
{
    /// <summary>
    /// Captures the data required to create a product.
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Product name shown in catalog views.
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [MinLength(3, ErrorMessage = "Product name must be at least 3 characters")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Brand that owns or manufactures the product.
        /// </summary>
        [Required(ErrorMessage = "Brand is required")]
        [MinLength(3, ErrorMessage = "Brand must be at least 3 characters")]
        [MaxLength(200, ErrorMessage = "Brand cannot exceed 200 characters")]
        public string Brand { get; set; } = String.Empty;

        /// <summary>
        /// Customer-facing product description.
        /// </summary>
        [Required(ErrorMessage = "Description is required")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = String.Empty;

        /// <summary>
        /// Category identifier the product belongs to.
        /// </summary>
        [Required(ErrorMessage = "Category ID is required")]
        public Guid CategoryId { get; set; }
    }
}
