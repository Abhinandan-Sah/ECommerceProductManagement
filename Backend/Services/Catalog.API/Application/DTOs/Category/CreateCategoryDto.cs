using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.Category
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        // Nullable because top-level categories won't have a parent
        public Guid? ParentCategoryId { get; set; }
    }
}
