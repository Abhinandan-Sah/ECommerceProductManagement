using Catalog.API.Application.DTOs.Category;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Exceptions;

namespace Catalog.API.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            _logger.LogInformation("Fetching all categories");

            var categories = await _repository.GetAllCategoriesAsync();

            return categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId
            });
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching category {CategoryId}", id);

            var category = await _repository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory != null ? category.ParentCategory.Name : "None"
            };
        }

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

            return new CategoryResponseDto
            {
                Id = savedCategory.Id,
                Name = savedCategory.Name,
                ParentCategoryId = savedCategory.ParentCategoryId
            };
        }

        public async Task UpdateCategoryAsync(Guid id, Category categoryUpdate)
        {
            _logger.LogInformation("Updating category {CategoryId}", id);

            var existingCategory = await _repository.GetCategoryByIdAsync(id);
            if (existingCategory == null) throw new NotFoundException("Category", id);

            existingCategory.Name = categoryUpdate.Name;
            await _repository.UpdateCategoryAsync(existingCategory);

            _logger.LogInformation("Category {CategoryId} updated successfully", id);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            _logger.LogInformation("Deleting category {CategoryId}", id);

            var category = await _repository.GetCategoryByIdAsync(id);
            if (category == null) throw new NotFoundException("Category", id);

            await _repository.DeleteCategoryAsync(category);

            _logger.LogInformation("Category {CategoryId} deleted successfully", id);
        }
    }
}
