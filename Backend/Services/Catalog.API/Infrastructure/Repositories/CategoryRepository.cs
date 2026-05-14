using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    /// <summary>
    /// Reads and writes category records from the catalog database.
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _context;

        /// <summary>
        /// Creates the category repository for the current catalog database context.
        /// </summary>
        public CategoryRepository(CatalogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core tracking and includes the parent category navigation property.</remarks>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core tracking and includes the parent category navigation property.</remarks>
        public async Task<Category?> GetCategoryByIdAsync(Guid Id)
        {
            return await _context.Categories.Include(c => c.ParentCategory).FirstOrDefaultAsync(c => c.Id == Id);
        }

        /// <inheritdoc />
        public async Task<Category> AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        /// <inheritdoc />
        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteCategoryAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
