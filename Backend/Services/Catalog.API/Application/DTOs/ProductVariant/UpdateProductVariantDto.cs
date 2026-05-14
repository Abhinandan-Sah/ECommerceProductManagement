using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.ProductVariant
{
    /// <summary>
    /// Captures the editable product variant fields accepted by update endpoints.
    /// </summary>
    public class UpdateProductVariantDto
    {
        /// <summary>
        /// Updated color option for the variant.
        /// </summary>
        [Required(ErrorMessage = "Color is required")]
        [MaxLength(100, ErrorMessage = "Color cannot exceed 100 characters")]
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Updated size option for the variant.
        /// </summary>
        [Required(ErrorMessage = "Size is required")]
        [MaxLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Updated scannable barcode value for the variant.
        /// </summary>
        [Required(ErrorMessage = "Barcode is required")]
        [MaxLength(100, ErrorMessage = "Barcode cannot exceed 100 characters")]
        public string Barcode { get; set; } = string.Empty;
    }
}
