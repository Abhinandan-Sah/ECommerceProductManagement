using System.ComponentModel.DataAnnotations;
using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand is required")]
        [MaxLength(200, ErrorMessage = "Brand cannot exceed 200 characters")]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category ID is required")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Publish status is required")]
        public PublishStatus PublishStatus { get; set; } 
    }
}
