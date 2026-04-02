using Catalog.API.Application.DTOs;
using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<IEnumerable<Category>> GetAllCategoriesAsync();
        public Task<Category?> GetCategoryByIdAsync(Guid Id);
        public Task<Category> AddCategoryAsync(Category category);
        public Task UpdateCategoryAsync(Category category);
        public Task DeleteCategoryAsync(Category category);
    }
}
