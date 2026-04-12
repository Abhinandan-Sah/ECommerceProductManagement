using Catalog.API.Application.DTOs.Product;
using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null, bool canViewUnpublished = false);
        Task<ProductResponseDto?> GetProductByIdAsync(Guid id, bool canViewUnpublished = false);
        Task<ProductResponseDto> AddProductAsync(CreateProductDto dto);
        Task UpdateProductAsync(Guid id, UpdateProductDto dto);
        Task DeleteProductAsync(Guid id);
    }
}
