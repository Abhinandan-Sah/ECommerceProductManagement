using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null);
        Task<Product?> GetProductByIdAsync(Guid Id);
        Task<Product> AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Product product);
    }
}
