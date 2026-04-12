using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.ProductVariant
{
    public class UpdateProductVariantDto
    {
        [Required(ErrorMessage = "Color is required")]
        [MaxLength(100, ErrorMessage = "Color cannot exceed 100 characters")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Size is required")]
        [MaxLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
        public string Size { get; set; } = string.Empty;

        [Required(ErrorMessage = "Barcode is required")]
        [MaxLength(100, ErrorMessage = "Barcode cannot exceed 100 characters")]
        public string Barcode { get; set; } = string.Empty;
    }
}
