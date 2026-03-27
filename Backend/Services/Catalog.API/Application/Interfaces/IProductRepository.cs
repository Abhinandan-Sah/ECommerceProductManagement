using Catalog.API.Application.DTOs;
using Catalog.API.Domain.Entities;

namespace Catalog.API.Application.Interfaces
{
    public interface IProductRepository
    {
        public Task<IEnumerable<Product>> GetAllProductsAsync();
        public Task<Product?> GetProductByIdAsync(Guid Id);
        public Task<Product> AddProductAsync(Product product);
    }
}
