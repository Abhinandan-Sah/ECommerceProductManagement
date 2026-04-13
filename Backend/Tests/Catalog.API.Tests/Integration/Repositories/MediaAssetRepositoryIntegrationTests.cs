using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Infrastructure.Data;
using Catalog.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Catalog.API.Tests.Integration.Repositories
{
    [TestFixture]
    public class MediaAssetRepositoryIntegrationTests
    {
        private CatalogDbContext _context = null!;
        private MediaAssetRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogDbContext(options);
            _repository = new MediaAssetRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddMediaAsync_ShouldSaveMediaToDatabase_AndGenerateId()
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

            var media = new MediaAsset
            {
                ProductId = product.Id,
                Url = "https://example.com/image.jpg",
                SortOrder = 1,
                AltText = "Laptop image"
            };

            // Act
            var result = await _repository.AddMediaAsync(media);

            // Assert
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            
            var savedMedia = await _context.MediaAssets.FindAsync(result.Id);
            Assert.That(savedMedia, Is.Not.Null);
            Assert.That(savedMedia!.ProductId, Is.EqualTo(product.Id));
            Assert.That(savedMedia.Url, Is.EqualTo("https://example.com/image.jpg"));
            Assert.That(savedMedia.SortOrder, Is.EqualTo(1));
            Assert.That(savedMedia.AltText, Is.EqualTo("Laptop image"));
        }

        [Test]
        public async Task GetMediaByProductIdAsync_ShouldReturnOnlyMediaForSpecificProduct_OrderedBySortOrder()
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

            var media1 = new MediaAsset
            {
                Id = Guid.NewGuid(),
                ProductId = product1.Id,
                Url = "https://example.com/image3.jpg",
                SortOrder = 3,
                AltText = "Third image"
            };

            var media2 = new MediaAsset
            {
                Id = Guid.NewGuid(),
                ProductId = product1.Id,
                Url = "https://example.com/image1.jpg",
                SortOrder = 1,
                AltText = "First image"
            };

            var media3 = new MediaAsset
            {
                Id = Guid.NewGuid(),
                ProductId = product1.Id,
                Url = "https://example.com/image2.jpg",
                SortOrder = 2,
                AltText = "Second image"
            };

            var media4 = new MediaAsset
            {
                Id = Guid.NewGuid(),
                ProductId = product2.Id,
                Url = "https://example.com/monitor.jpg",
                SortOrder = 1,
                AltText = "Monitor image"
            };

            await _context.MediaAssets.AddRangeAsync(media1, media2, media3, media4);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetMediaByProductIdAsync(product1.Id);

            // Assert
            var mediaList = results.ToList();
            Assert.That(mediaList, Has.Count.EqualTo(3));
            Assert.That(mediaList.All(m => m.ProductId == product1.Id), Is.True);
            Assert.That(mediaList[0].SortOrder, Is.EqualTo(1));
            Assert.That(mediaList[1].SortOrder, Is.EqualTo(2));
            Assert.That(mediaList[2].SortOrder, Is.EqualTo(3));
            Assert.That(mediaList[0].AltText, Is.EqualTo("First image"));
            Assert.That(mediaList[1].AltText, Is.EqualTo("Second image"));
            Assert.That(mediaList[2].AltText, Is.EqualTo("Third image"));
        }
    }
}
