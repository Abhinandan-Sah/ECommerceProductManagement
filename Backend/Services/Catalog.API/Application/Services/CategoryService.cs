using Catalog.API.Application.DTOs.Category;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
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
            var categoryEntity = new Category
            {
                Name = dto.Name,
                ParentCategoryId = dto.ParentCategoryId
            };

            var savedCategory = await _repository.AddCategoryAsync(categoryEntity);

            return new CategoryResponseDto
            {
                Id = savedCategory.Id,
                Name = savedCategory.Name,
                ParentCategoryId = savedCategory.ParentCategoryId
            };
        }

        public async Task UpdateCategoryAsync(Guid id, Category categoryUpdate)
        {
            var existingCategory = await _repository.GetCategoryByIdAsync(id);
            if (existingCategory == null) throw new InvalidOperationException("Category not found.");

            existingCategory.Name = categoryUpdate.Name;
            await _repository.UpdateCategoryAsync(existingCategory);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _repository.GetCategoryByIdAsync(id);
            if (category == null) throw new InvalidOperationException("Category not found.");

            await _repository.DeleteCategoryAsync(category);
        }
    }
}
