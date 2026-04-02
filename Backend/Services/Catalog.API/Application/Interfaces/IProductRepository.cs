using Catalog.API.Application.DTOs;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;

namespace Catalog.API.Application.Interfaces
{
    public interface IProductRepository
    {
        public Task<IEnumerable<Product>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null);
        public Task<Product?> GetProductByIdAsync(Guid Id);
        public Task<Product> AddProductAsync(Product product);
        public Task UpdateProductAsync(Product product);
        public Task DeleteProductAsync(Product product);
    }
}
