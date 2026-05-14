using Catalog.API.Application.DTOs.Category;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Exceptions;

namespace Catalog.API.Application.Services
{
    /// <summary>
    /// Applies category rules and shapes category data for API responses.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ILogger<CategoryService> _logger;

        /// <summary>
        /// Creates the category service with its repository and logger.
        /// </summary>
        public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            _logger.LogInformation("Fetching all categories");

            var categories = await _repository.GetAllCategoriesAsync();

            return categories.Select(category => MapToDto(category));
        }

        /// <inheritdoc />
        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching category {CategoryId}", id);

            var category = await _repository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            return MapToDto(category);
        }

        /// <inheritdoc />
        public async Task<CategoryResponseDto> AddCategoryAsync(CreateCategoryDto dto)
        {
            _logger.LogInformation("Adding new category: {CategoryName}", dto.Name);

            var categoryEntity = new Category
            {
                Name = dto.Name,
                ParentCategoryId = dto.ParentCategoryId
            };

            var savedCategory = await _repository.AddCategoryAsync(categoryEntity);

            _logger.LogInformation("Category {CategoryId} created successfully", savedCategory.Id);

            return MapToDto(savedCategory, missingParentName: string.Empty);
        }

        /// <inheritdoc />
        public async Task UpdateCategoryAsync(Guid id, Category categoryUpdate)
        {
            _logger.LogInformation("Updating category {CategoryId}", id);

            var existingCategory = await _repository.GetCategoryByIdAsync(id);
            if (existingCategory == null) throw new NotFoundException("Category", id);

            existingCategory.Name = categoryUpdate.Name;
            await _repository.UpdateCategoryAsync(existingCategory);

            _logger.LogInformation("Category {CategoryId} updated successfully", id);
        }

        /// <inheritdoc />
        public async Task DeleteCategoryAsync(Guid id)
        {
            _logger.LogInformation("Deleting category {CategoryId}", id);

            var category = await _repository.GetCategoryByIdAsync(id);
            if (category == null) throw new NotFoundException("Category", id);

            await _repository.DeleteCategoryAsync(category);

            _logger.LogInformation("Category {CategoryId} deleted successfully", id);
        }

        /// <summary>
        /// Converts a category entity into the response shape used by category endpoints.
        /// </summary>
        private static CategoryResponseDto MapToDto(Category category, string missingParentName = "None") => new()
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory != null ? category.ParentCategory.Name : missingParentName
        };
    }
}
