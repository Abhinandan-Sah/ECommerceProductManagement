using Catalog.API.Application.DTOs.MediaAsset;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Catalog.API.Tests.Unit.Application.Services
{
    [TestFixture]
    [Category("Catalog")]
    [Category("Unit")]
    [Category("MediaAssetService")]
    [Author("Catalog.API Team")]
    public class MediaAssetServiceTests
    {
        private Mock<IMediaAssetRepository> _mockMediaAssetRepository = null!;
        private Mock<IProductRepository> _mockProductRepository = null!;
        private Mock<ILogger<MediaAssetService>> _mockLogger = null!;
        private MediaAssetService _mediaAssetService = null!;

        [SetUp]
        public void SetUp()
        {
            _mockMediaAssetRepository = new Mock<IMediaAssetRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<MediaAssetService>>();
            
            _mediaAssetService = new MediaAssetService(
                _mockMediaAssetRepository.Object,
                _mockProductRepository.Object,
                _mockLogger.Object
            );
        }

        [Test]
        [Description("Verifies that shoppers can retrieve the media gallery for a published product.")]
        public async Task GetMediaByProductAsync_ReturnsMediaAssets()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var publishedProduct = new Product { Id = productId, PublishStatus = PublishStatus.Published };
            var mediaAssets = new List<MediaAsset>
            {
                new MediaAsset { Id = Guid.NewGuid(), ProductId = productId, Url = "url1.jpg", SortOrder = 1, AltText = "Alt 1" },
                new MediaAsset { Id = Guid.NewGuid(), ProductId = productId, Url = "url2.jpg", SortOrder = 2, AltText = "Alt 2" }
            };

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync(publishedProduct);

            _mockMediaAssetRepository
                .Setup(r => r.GetMediaByProductIdAsync(productId))
                .ReturnsAsync(mediaAssets);

            // Act
            var result = await _mediaAssetService.GetMediaByProductAsync(productId, role: null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        [Description("Verifies that a known media asset returns its product-scoped image metadata.")]
        public async Task GetMediaByIdAsync_ExistingMedia_ReturnsMediaAsset()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var mediaId = Guid.NewGuid();
            var publishedProduct = new Product { Id = productId, PublishStatus = PublishStatus.Published };
            var mediaAsset = new MediaAsset
            {
                Id = mediaId,
                ProductId = productId,
                Url = "test.jpg",
                SortOrder = 1,
                AltText = "Test Alt"
            };

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync(publishedProduct);

            _mockMediaAssetRepository
                .Setup(r => r.GetMediaByIdAsync(mediaId))
                .ReturnsAsync(mediaAsset);

            // Act
            var result = await _mediaAssetService.GetMediaByIdAsync(productId, mediaId, role: null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(mediaId));
            Assert.That(result.ProductId, Is.EqualTo(productId));
            Assert.That(result.Url, Is.EqualTo("test.jpg"));
            Assert.That(result.SortOrder, Is.EqualTo(1));
            Assert.That(result.AltText, Is.EqualTo("Test Alt"));
        }

        [Test]
        [Description("Verifies that missing media assets are returned as null for product media lookups.")]
        public async Task GetMediaByIdAsync_NonExistentMedia_ReturnsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var mediaId = Guid.NewGuid();
            var publishedProduct = new Product { Id = productId, PublishStatus = PublishStatus.Published };

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync(publishedProduct);

            _mockMediaAssetRepository
                .Setup(r => r.GetMediaByIdAsync(mediaId))
                .ReturnsAsync((MediaAsset?)null);

            // Act
            var result = await _mediaAssetService.GetMediaByIdAsync(productId, mediaId, role: null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Description("Verifies that valid media input creates a product image with sort and accessibility metadata.")]
        public async Task AddMediaAsync_ValidData_CreatesMediaAsset()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var createDto = new CreateMediaAssetDto
            {
                Url = "new-image.jpg",
                SortOrder = 1,
                AltText = "New Image"
            };

            var savedMediaAsset = new MediaAsset
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Url = createDto.Url,
                SortOrder = createDto.SortOrder,
                AltText = createDto.AltText
            };

            _mockMediaAssetRepository
                .Setup(r => r.AddMediaAsync(It.IsAny<MediaAsset>()))
                .ReturnsAsync(savedMediaAsset);

            // Act
            var result = await _mediaAssetService.AddMediaAsync(productId, createDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductId, Is.EqualTo(productId));
            Assert.That(result.Url, Is.EqualTo("new-image.jpg"));
            Assert.That(result.SortOrder, Is.EqualTo(1));
            Assert.That(result.AltText, Is.EqualTo("New Image"));

            _mockMediaAssetRepository.Verify(r => r.AddMediaAsync(It.IsAny<MediaAsset>()), Times.Once);
        }

        [Test]
        [Description("Verifies that deleting a missing media asset fails with a not-found error.")]
        public async Task DeleteMediaAsync_NonExistentMedia_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var mediaId = Guid.NewGuid();

            _mockMediaAssetRepository
                .Setup(r => r.GetMediaByIdAsync(mediaId))
                .ReturnsAsync((MediaAsset?)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotFoundException>(
                async () => await _mediaAssetService.DeleteMediaAsync(productId, mediaId)
            );
            Assert.That(exception!.Message, Does.Contain("MediaAsset"));
        }
    }
}
