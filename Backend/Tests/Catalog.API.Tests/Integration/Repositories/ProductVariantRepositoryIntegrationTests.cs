using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Infrastructure.Data;
using Catalog.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Catalog.API.Tests.Integration.Repositories
{
    [TestFixture]
    public class ProductVariantRepositoryIntegrationTests
    {
        private CatalogDbContext _context = null!;
        private ProductVariantRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogDbContext(options);
            _repository = new ProductVariantRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddVariantAsync_ShouldSaveVariantToDatabase_AndGenerateId()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics"
            };
            await _context.Categories.AddAsync(category);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop",
                Brand = "Dell",
                Description = "High-performance laptop",
                SKU = "DELL-001",
                PublishStatus = PublishStatus.Draft,
                CategoryId = category.Id
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var variant = new ProductVariant
            {
                ProductId = product.Id,
                Color = "Black",
                Size = "15-inch",
                Barcode = "123456789"
            };

            // Act
            var result = await _repository.AddVariantAsync(variant);

            // Assert
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            
            var savedVariant = await _context.ProductVariants.FindAsync(result.Id);
            Assert.That(savedVariant, Is.Not.Null);
            Assert.That(savedVariant!.ProductId, Is.EqualTo(product.Id));
            Assert.That(savedVariant.Color, Is.EqualTo("Black"));
            Assert.That(savedVariant.Size, Is.EqualTo("15-inch"));
            Assert.That(savedVariant.Barcode, Is.EqualTo("123456789"));
        }

        [Test]
        public async Task GetVariantsByProductIdAsync_ShouldReturnOnlyVariantsForSpecificProduct()
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
                Name = "Laptop",
                Brand = "Dell",
                Description = "High-performance laptop",
                SKU = "DELL-001",
                PublishStatus = PublishStatus.Draft,
                CategoryId = category.Id
            };

            var product2 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Monitor",
                Brand = "Samsung",
                Description = "4K Monitor",
                SKU = "SAM-001",
                PublishStatus = PublishStatus.Draft,
                CategoryId = category.Id
            };

            await _context.Products.AddRangeAsync(product1, product2);
            await _context.SaveChangesAsync();

            var variant1 = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product1.Id,
                Color = "Black",
                Size = "15-inch",
                Barcode = "123456789"
            };

            var variant2 = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product1.Id,
                Color = "Silver",
                Size = "15-inch",
                Barcode = "987654321"
            };

            var variant3 = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product2.Id,
                Color = "Black",
                Size = "27-inch",
                Barcode = "555555555"
            };

            await _context.ProductVariants.AddRangeAsync(variant1, variant2, variant3);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetVariantsByProductIdAsync(product1.Id);

            // Assert
            var variantList = results.ToList();
            Assert.That(variantList, Has.Count.EqualTo(2));
            Assert.That(variantList.All(v => v.ProductId == product1.Id), Is.True);
            Assert.That(variantList.Any(v => v.Color == "Black"), Is.True);
            Assert.That(variantList.Any(v => v.Color == "Silver"), Is.True);
        }
    }
}
