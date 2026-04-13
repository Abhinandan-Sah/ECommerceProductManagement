using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Infrastructure.Data;
using Catalog.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Catalog.API.Tests.Integration.Repositories
{
    [TestFixture]
    public class ProductRepositoryIntegrationTests
    {
        private CatalogDbContext _context = null!;
        private ProductRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogDbContext(options);
            _repository = new ProductRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddProductAsync_ShouldSaveProductToDatabase_AndGenerateId()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Laptop",
                Brand = "Dell",
                Description = "High-performance laptop",
                SKU = "DELL-001",
                PublishStatus = PublishStatus.Draft,
                CategoryId = category.Id
            };

            // Act
            var result = await _repository.AddProductAsync(product);

            // Assert
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            
            var savedProduct = await _context.Products.FindAsync(result.Id);
            Assert.That(savedProduct, Is.Not.Null);
            Assert.That(savedProduct!.Name, Is.EqualTo("Laptop"));
            Assert.That(savedProduct.Brand, Is.EqualTo("Dell"));
            Assert.That(savedProduct.SKU, Is.EqualTo("DELL-001"));
            Assert.That(savedProduct.CategoryId, Is.EqualTo(category.Id));
        }

        [Test]
        public async Task GetAllProductsAsync_WithStatusFilter_ShouldReturnOnlyMatchingProducts()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics"
            };
            await _context.Categories.AddAsync(category);

            var product1 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Brand = "Brand A",
                Description = "Description 1",
                SKU = "SKU-001",
                PublishStatus = PublishStatus.Published,
                CategoryId = category.Id
            };

            var product2 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Brand = "Brand B",
                Description = "Description 2",
                SKU = "SKU-002",
                PublishStatus = PublishStatus.Draft,
                CategoryId = category.Id
            };

            var product3 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 3",
                Brand = "Brand C",
                Description = "Description 3",
                SKU = "SKU-003",
                PublishStatus = PublishStatus.Published,
                CategoryId = category.Id
            };

            await _context.Products.AddRangeAsync(product1, product2, product3);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetAllProductsAsync(status: PublishStatus.Published);

            // Assert
            var productList = results.ToList();
            Assert.That(productList, Has.Count.EqualTo(2));
            Assert.That(productList.All(p => p.PublishStatus == PublishStatus.Published), Is.True);
            Assert.That(productList.Any(p => p.Name == "Product 1"), Is.True);
            Assert.That(productList.Any(p => p.Name == "Product 3"), Is.True);
            Assert.That(productList.Any(p => p.Name == "Product 2"), Is.False);
        }
    }
}
