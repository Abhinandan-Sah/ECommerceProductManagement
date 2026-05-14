using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Application.DTOs.Category
{
    /// <summary>
    /// Captures the data required to create a category.
    /// </summary>
    public class CreateCategoryDto
    {
        /// <summary>
        /// Category display name shown to catalog users.
        /// </summary>
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(200, ErrorMessage = "Category name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional parent category identifier; null means the category is top-level.
        /// </summary>
        public Guid? ParentCategoryId { get; set; }
    }
}
