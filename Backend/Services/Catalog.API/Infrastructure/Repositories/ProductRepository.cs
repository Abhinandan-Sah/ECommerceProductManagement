using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
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

        public async Task<IEnumerable<Product>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null)
        {
            // 1. Start tracking the query, but DO NOT execute it yet
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // 2. If a category ID was provided, add a WHERE clause
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // 3. If a status was provided, add another WHERE clause
            if (status.HasValue)
            {
                query = query.Where(p => p.PublishStatus == status.Value);
            }

            // 4. Finally, execute the highly optimized SQL query and return the list
            return await query.ToListAsync();
        }
        public async Task<Product?> GetProductByIdAsync(Guid Id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == Id);
        }
        public async Task<Product> AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetSkusByPrefixAsync(string prefix)
        {
            return await _context.Products
                .Where(p => p.SKU.StartsWith(prefix))
                .Select(p => p.SKU)
                .ToListAsync();
        }
    }
}
