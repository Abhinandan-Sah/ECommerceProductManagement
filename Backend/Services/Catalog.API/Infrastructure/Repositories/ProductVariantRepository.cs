using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly CatalogDbContext _context;

        public ProductVariantRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetVariantByIdAsync(Guid id)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<ProductVariant> AddVariantAsync(ProductVariant variant)
        {
            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task UpdateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
        }
    }
}
