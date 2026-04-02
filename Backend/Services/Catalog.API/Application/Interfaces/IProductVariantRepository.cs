using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces
{
    public interface IProductVariantRepository
    {
        Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId);
        Task<ProductVariant?> GetVariantByIdAsync(Guid id);
        Task<ProductVariant> AddVariantAsync(ProductVariant variant);
        Task UpdateVariantAsync(ProductVariant variant);
        Task DeleteVariantAsync(ProductVariant variant);
    }
}
