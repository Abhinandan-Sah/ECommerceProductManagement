using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    /// <summary>
    /// Reads and writes product variant records from the catalog database.
    /// </summary>
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly CatalogDbContext _context;

        /// <summary>
        /// Creates the product variant repository for the current catalog database context.
        /// </summary>
        public ProductVariantRepository(CatalogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core tracking and includes the product navigation property.</remarks>
        public async Task<ProductVariant?> GetVariantByIdAsync(Guid id)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        /// <inheritdoc />
        public async Task<ProductVariant> AddVariantAsync(ProductVariant variant)
        {
            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        /// <inheritdoc />
        public async Task UpdateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
        }
    }
}
