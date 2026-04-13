using Catalog.API.Application.DTOs.ProductVariant;
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
    public class ProductVariantServiceTests
    {
        private Mock<IProductVariantRepository> _mockProductVariantRepository = null!;
        private Mock<IProductRepository> _mockProductRepository = null!;
        private Mock<ILogger<ProductVariantService>> _mockLogger = null!;
        private ProductVariantService _productVariantService = null!;

        [SetUp]
        public void SetUp()
        {
            _mockProductVariantRepository = new Mock<IProductVariantRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<ProductVariantService>>();
            
            _productVariantService = new ProductVariantService(
                _mockProductVariantRepository.Object,
                _mockProductRepository.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task GetVariantsByProductAsync_ReturnsVariants()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var variants = new List<ProductVariant>
            {
                new ProductVariant { Id = Guid.NewGuid(), ProductId = productId, Color = "Red", Size = "M", Barcode = "123" },
                new ProductVariant { Id = Guid.NewGuid(), ProductId = productId, Color = "Blue", Size = "L", Barcode = "456" }
            };

            _mockProductVariantRepository
                .Setup(r => r.GetVariantsByProductIdAsync(productId))
                .ReturnsAsync(variants);

            // Act
            var result = await _productVariantService.GetVariantsByProductAsync(productId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetVariantByIdAsync_ExistingVariant_ReturnsVariant()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var variant = new ProductVariant
            {
                Id = variantId,
                ProductId = productId,
                Color = "Red",
                Size = "M",
                Barcode = "123"
            };

            _mockProductVariantRepository
                .Setup(r => r.GetVariantByIdAsync(variantId))
                .ReturnsAsync(variant);

            // Act
            var result = await _productVariantService.GetVariantByIdAsync(productId, variantId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(variantId));
            Assert.That(result.ProductId, Is.EqualTo(productId));
            Assert.That(result.Color, Is.EqualTo("Red"));
            Assert.That(result.Size, Is.EqualTo("M"));
            Assert.That(result.Barcode, Is.EqualTo("123"));
        }

        [Test]
        public async Task GetVariantByIdAsync_NonExistentVariant_ReturnsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            _mockProductVariantRepository
                .Setup(r => r.GetVariantByIdAsync(variantId))
                .ReturnsAsync((ProductVariant?)null);

            // Act
            var result = await _productVariantService.GetVariantByIdAsync(productId, variantId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddVariantAsync_InvalidProduct_ThrowsBadRequestException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var createDto = new CreateProductVariantDto
            {
                Color = "Red",
                Size = "M",
                Barcode = "123"
            };

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<BadRequestException>(
                async () => await _productVariantService.AddVariantAsync(productId, createDto)
            );
            Assert.That(exception!.Message, Does.Contain($"Product with ID {productId} does not exist"));
        }

        [Test]
        public async Task AddVariantAsync_ValidData_CreatesVariant()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Test Product" };
            var createDto = new CreateProductVariantDto
            {
                Color = "Red",
                Size = "M",
                Barcode = "123"
            };

            var savedVariant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Color = createDto.Color,
                Size = createDto.Size,
                Barcode = createDto.Barcode
            };

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            _mockProductVariantRepository
                .Setup(r => r.AddVariantAsync(It.IsAny<ProductVariant>()))
                .ReturnsAsync(savedVariant);

            // Act
            var result = await _productVariantService.AddVariantAsync(productId, createDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductId, Is.EqualTo(productId));
            Assert.That(result.Color, Is.EqualTo("Red"));
            Assert.That(result.Size, Is.EqualTo("M"));
            Assert.That(result.Barcode, Is.EqualTo("123"));

            _mockProductVariantRepository.Verify(r => r.AddVariantAsync(It.IsAny<ProductVariant>()), Times.Once);
        }

        [Test]
        public async Task UpdateVariantAsync_NonExistentVariant_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var updateDto = new UpdateProductVariantDto
            {
                Color = "Blue",
                Size = "L",
                Barcode = "456"
            };

            _mockProductVariantRepository
                .Setup(r => r.GetVariantByIdAsync(variantId))
                .ReturnsAsync((ProductVariant?)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotFoundException>(
                async () => await _productVariantService.UpdateVariantAsync(productId, variantId, updateDto)
            );
            Assert.That(exception!.Message, Does.Contain("ProductVariant"));
        }

        [Test]
        public async Task DeleteVariantAsync_NonExistentVariant_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            _mockProductVariantRepository
                .Setup(r => r.GetVariantByIdAsync(variantId))
                .ReturnsAsync((ProductVariant?)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotFoundException>(
                async () => await _productVariantService.DeleteVariantAsync(productId, variantId)
            );
            Assert.That(exception!.Message, Does.Contain("ProductVariant"));
        }
    }
}
