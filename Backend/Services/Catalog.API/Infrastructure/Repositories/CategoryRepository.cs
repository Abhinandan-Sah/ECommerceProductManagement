using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _context;
        public CategoryRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
        }
        public async Task<Category?> GetCategoryByIdAsync(Guid Id)
        {
            return await _context.Categories.Include(c => c.ParentCategory).FirstOrDefaultAsync(c => c.Id == Id);
        }
        public async Task<Category> AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteCategoryAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
