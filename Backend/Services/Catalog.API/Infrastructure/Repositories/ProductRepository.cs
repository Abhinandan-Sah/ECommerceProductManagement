using Catalog.API.Application.Interfaces;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;
        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }
        public async Task<Product?> GetProductByIdAsync(Guid Id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == Id);
        }
        public async Task<Product> AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
