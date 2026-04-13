using Catalog.API.Application.DTOs.MediaAsset;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Services;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Catalog.API.Tests.Unit.Application.Services
{
    [TestFixture]
    public class MediaAssetServiceTests
    {
        private Mock<IMediaAssetRepository> _mockMediaAssetRepository = null!;
        private Mock<ILogger<MediaAssetService>> _mockLogger = null!;
        private MediaAssetService _mediaAssetService = null!;

        [SetUp]
        public void SetUp()
        {
            _mockMediaAssetRepository = new Mock<IMediaAssetRepository>();
            _mockLogger = new Mock<ILogger<MediaAssetService>>();
            
            _mediaAssetService = new MediaAssetService(
                _mockMediaAssetRepository.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task GetMediaByProductAsync_ReturnsMediaAssets()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var mediaAssets = new List<MediaAsset>
            {
                new MediaAsset { Id = Guid.NewGuid(), ProductId = productId, Url = "url1.jpg", SortOrder = 1, AltText = "Alt 1" },
                new MediaAsset { Id = Guid.NewGuid(), ProductId = productId, Url = "url2.jpg", SortOrder = 2, AltText = "Alt 2" }
            };

            _mockMediaAssetRepository
                .Setup(r => r.GetMediaByProductIdAsync(productId))
                .ReturnsAsync(mediaAssets);

            // Act
            var result = await _mediaAssetService.GetMediaByProductAsync(productId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetMediaByIdAsync_ExistingMedia_ReturnsMediaAsset()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var mediaId = Guid.NewGuid();
            var mediaAsset = new MediaAsset
            {
                Id = mediaId,
                ProductId = productId,
                Url = "test.jpg",
                SortOrder = 1,
                AltText = "Test Alt"
            };

            _mockMediaAssetRepository
                .Setup(r => r.GetMediaByIdAsync(mediaId))
                .ReturnsAsync(mediaAsset);

            // Act
            var result = await _mediaAssetService.GetMediaByIdAsync(productId, mediaId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(mediaId));
            Assert.That(result.ProductId, Is.EqualTo(productId));
            Assert.That(result.Url, Is.EqualTo("test.jpg"));
            Assert.That(result.SortOrder, Is.EqualTo(1));
            Assert.That(result.AltText, Is.EqualTo("Test Alt"));
        }

        [Test]
        public async Task GetMediaByIdAsync_NonExistentMedia_ReturnsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var mediaId = Guid.NewGuid();

            _mockMediaAssetRepository
                .Setup(r => r.GetMediaByIdAsync(mediaId))
                .ReturnsAsync((MediaAsset?)null);

            // Act
            var result = await _mediaAssetService.GetMediaByIdAsync(productId, mediaId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
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
