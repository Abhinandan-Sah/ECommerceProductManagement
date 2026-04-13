using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.Product
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = String.Empty;

        [Required(ErrorMessage = "Brand is required")]
        [MaxLength(200, ErrorMessage = "Brand cannot exceed 200 characters")]
        public string Brand { get; set; } = String.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = String.Empty;

        [Required(ErrorMessage = "Category ID is required")]
        public Guid CategoryId { get; set; }
    }
}
