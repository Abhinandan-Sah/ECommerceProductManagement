using Catalog.API.Application.DTOs.Category;
using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces.Services
{
    /// <summary>
    /// Defines category business operations.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Gets the catalog category tree in a flat response list.
        /// </summary>
        /// <returns>All categories with parent category display names when available.</returns>
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();

        /// <summary>
        /// Gets one category, including its parent category name when it has one.
        /// </summary>
        /// <param name="id">Category identifier to load.</param>
        /// <returns>The matching category response, or null when the category does not exist.</returns>
        Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id);

        /// <summary>
        /// Creates a category and optionally links it to a parent category.
        /// </summary>
        /// <param name="dto">Category details supplied by the caller.</param>
        /// <returns>The created category response.</returns>
        Task<CategoryResponseDto> AddCategoryAsync(CreateCategoryDto dto);

        /// <summary>
        /// Renames an existing category.
        /// </summary>
        /// <param name="id">Category identifier to update.</param>
        /// <param name="category">New category values.</param>
        Task UpdateCategoryAsync(Guid id, Category category);

        /// <summary>
        /// Removes an existing category.
        /// </summary>
        /// <param name="id">Category identifier to delete.</param>
        Task DeleteCategoryAsync(Guid id);
    }
}
