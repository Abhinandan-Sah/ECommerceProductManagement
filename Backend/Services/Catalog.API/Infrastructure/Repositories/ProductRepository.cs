using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    /// <summary>
    /// Reads and writes product records from the catalog database.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;

        /// <summary>
        /// Creates the product repository for the current catalog database context.
        /// </summary>
        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <remarks>Builds the EF Core query incrementally so only requested filters are sent to SQL.</remarks>
        public async Task<IEnumerable<Product>> GetAllProductsAsync(Guid? categoryId = null, PublishStatus? status = null)
        {
            // Build the query step by step so EF sends only the requested filters to SQL.
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.PublishStatus == status.Value);
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core tracking and includes the category navigation property.</remarks>
        public async Task<Product?> GetProductByIdAsync(Guid Id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == Id);
        }

        /// <inheritdoc />
        public async Task<Product> AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        /// <inheritdoc />
        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteProductAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetSkusByPrefixAsync(string prefix)
        {
            // SKU generation asks for the existing suffixes with the same prefix before choosing the next one.
            return await _context.Products
                .Where(p => p.SKU.StartsWith(prefix))
                .Select(p => p.SKU)
                .ToListAsync();
        }
    }
}
