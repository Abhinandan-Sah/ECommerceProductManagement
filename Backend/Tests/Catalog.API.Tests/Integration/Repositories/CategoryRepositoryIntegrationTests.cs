using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;
using Catalog.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Catalog.API.Tests.Integration.Repositories
{
    [TestFixture]
    public class CategoryRepositoryIntegrationTests
    {
        private CatalogDbContext _context = null!;
        private CategoryRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogDbContext(options);
            _repository = new CategoryRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddCategoryAsync_ShouldSaveCategoryToDatabase_AndGenerateId()
        {
            // Arrange
            var category = new Category
            {
                Name = "Electronics"
            };

            // Act
            var result = await _repository.AddCategoryAsync(category);

            // Assert
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            
            var savedCategory = await _context.Categories.FindAsync(result.Id);
            Assert.That(savedCategory, Is.Not.Null);
            Assert.That(savedCategory!.Name, Is.EqualTo("Electronics"));
        }

        [Test]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var category1 = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics"
            };

            var category2 = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Clothing"
            };

            var category3 = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Books"
            };

            await _context.Categories.AddRangeAsync(category1, category2, category3);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetAllCategoriesAsync();

            // Assert
            var categoryList = results.ToList();
            Assert.That(categoryList, Has.Count.EqualTo(3));
            Assert.That(categoryList.Any(c => c.Name == "Electronics"), Is.True);
            Assert.That(categoryList.Any(c => c.Name == "Clothing"), Is.True);
            Assert.That(categoryList.Any(c => c.Name == "Books"), Is.True);
        }
    }
}
