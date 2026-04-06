using Catalog.API.Application.DTOs.ProductVariant;

namespace Catalog.API.Application.Interfaces.Services
{
    public interface IProductVariantService
    {
        Task<IEnumerable<ProductVariantResponseDto>> GetVariantsByProductAsync(Guid productId);
        Task<ProductVariantResponseDto?> GetVariantByIdAsync(Guid productId, Guid id);
        Task<ProductVariantResponseDto> AddVariantAsync(Guid productId, CreateProductVariantDto dto);
        Task UpdateVariantAsync(Guid productId, Guid id, UpdateProductVariantDto dto);
        Task DeleteVariantAsync(Guid productId, Guid id);
    }
}
