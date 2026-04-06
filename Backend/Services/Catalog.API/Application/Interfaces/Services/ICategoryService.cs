using Catalog.API.Application.DTOs.Category;
using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id);
        Task<CategoryResponseDto> AddCategoryAsync(CreateCategoryDto dto);
        Task UpdateCategoryAsync(Guid id, Category category);
        Task DeleteCategoryAsync(Guid id);
    }
}
