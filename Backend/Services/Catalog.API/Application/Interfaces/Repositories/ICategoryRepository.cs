using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines tracked category persistence operations for the catalog database.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Reads all categories with their parent category loaded.
        /// </summary>
        /// <returns>Tracked category entities with parent category navigation data, or an empty collection when no categories exist.</returns>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Finds one category with its parent category loaded.
        /// </summary>
        /// <param name="id">Category identifier to load.</param>
        /// <returns>The tracked matching category, or null when it does not exist.</returns>
        Task<Category?> GetCategoryByIdAsync(Guid id);

        /// <summary>
        /// Adds a new category and saves the database change immediately.
        /// </summary>
        /// <param name="category">Category entity to add.</param>
        /// <returns>The tracked saved category entity.</returns>
        Task<Category> AddCategoryAsync(Category category);

        /// <summary>
        /// Updates a category and saves the database change immediately.
        /// </summary>
        /// <param name="category">Category entity with updated values.</param>
        Task UpdateCategoryAsync(Category category);

        /// <summary>
        /// Removes a category and saves the database change immediately.
        /// </summary>
        /// <param name="category">Category entity to remove.</param>
        Task DeleteCategoryAsync(Category category);
    }
}
