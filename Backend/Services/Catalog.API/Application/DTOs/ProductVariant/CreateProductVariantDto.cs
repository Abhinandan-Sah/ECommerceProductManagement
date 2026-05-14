using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.ProductVariant
{
    /// <summary>
    /// Captures the data required to create a product variant.
    /// </summary>
    public class CreateProductVariantDto
    {
        /// <summary>
        /// Color option for the variant.
        /// </summary>
        [Required(ErrorMessage = "Color is required")]
        [MaxLength(100, ErrorMessage = "Color cannot exceed 100 characters")]
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Size option for the variant.
        /// </summary>
        [Required(ErrorMessage = "Size is required")]
        [MaxLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Scannable barcode value for the variant.
        /// </summary>
        [Required(ErrorMessage = "Barcode is required")]
        [MaxLength(100, ErrorMessage = "Barcode cannot exceed 100 characters")]
        public string Barcode { get; set; } = string.Empty;
    }
}
