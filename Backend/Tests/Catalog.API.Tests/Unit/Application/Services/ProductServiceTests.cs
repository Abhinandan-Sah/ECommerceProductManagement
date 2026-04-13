using Catalog.API.Application.DTOs.Product;
using Catalog.API.Application.Interfaces.Repositories;
using Catalog.API.Application.Interfaces.Services;
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
    public class ProductServiceTests
    {
        private Mock<IProductRepository> _mockProductRepository = null!;
        private Mock<ICategoryRepository> _mockCategoryRepository = null!;
        private Mock<ISkuGenerator> _mockSkuGenerator = null!;
        private Mock<ILogger<ProductService>> _mockLogger = null!;
        private ProductService _productService = null!;

        [SetUp]
        public void SetUp()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockSkuGenerator = new Mock<ISkuGenerator>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            
            _productService = new ProductService(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object,
                _mockSkuGenerator.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task GetProductByIdAsync_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                SKU = "TEST-001",
                Brand = "Test Brand",
                Description = "Test Description",
                PublishStatus = PublishStatus.Published,
                CategoryId = categoryId,
                Category = new Category { Id = categoryId, Name = "Test Category" }
            };

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId, canViewUnpublished: false);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(productId));
            Assert.That(result.Name, Is.EqualTo("Test Product"));
            Assert.That(result.SKU, Is.EqualTo("TEST-001"));
            Assert.That(result.Brand, Is.EqualTo("Test Brand"));
            Assert.That(result.Description, Is.EqualTo("Test Description"));
            Assert.That(result.PublishStatus, Is.EqualTo(PublishStatus.Published));
            Assert.That(result.CategoryName, Is.EqualTo("Test Category"));
        }

        [Test]
        public async Task GetProductByIdAsync_NonExistentProduct_ReturnsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(productId, canViewUnpublished: false);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddProductAsync_InvalidCategory_ThrowsBadRequestException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Brand = "New Brand",
                Description = "New Description",
                CategoryId = categoryId
            };

            _mockCategoryRepository
                .Setup(r => r.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<BadRequestException>(
                async () => await _productService.AddProductAsync(createDto)
            );
            Assert.That(exception!.Message, Does.Contain($"Category with ID {categoryId} does not exist"));
        }

        [Test]
        public async Task AddProductAsync_ValidData_CreatesProduct()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category { Id = categoryId, Name = "Valid Category" };
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Brand = "New Brand",
                Description = "New Description",
                CategoryId = categoryId
            };

            var generatedSku = "NEW-BRAND-NEW-PRODUCT-001";

            var savedProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                SKU = generatedSku,
                Brand = createDto.Brand,
                Description = createDto.Description,
                CategoryId = categoryId,
                PublishStatus = PublishStatus.Draft,
                Category = category
            };

            _mockCategoryRepository
                .Setup(r => r.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockSkuGenerator
                .Setup(s => s.GenerateSkuAsync(createDto.Brand, createDto.Name))
                .ReturnsAsync(generatedSku);

            _mockProductRepository
                .Setup(r => r.AddProductAsync(It.IsAny<Product>()))
                .ReturnsAsync(savedProduct);

            // Act
            var result = await _productService.AddProductAsync(createDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("New Product"));
            Assert.That(result.SKU, Is.EqualTo(generatedSku));
            Assert.That(result.Brand, Is.EqualTo("New Brand"));
            Assert.That(result.Description, Is.EqualTo("New Description"));
            Assert.That(result.PublishStatus, Is.EqualTo(PublishStatus.Draft));
            Assert.That(result.CategoryName, Is.EqualTo("Valid Category"));

            _mockSkuGenerator.Verify(s => s.GenerateSkuAsync(createDto.Brand, createDto.Name), Times.Once);
            _mockProductRepository.Verify(r => r.AddProductAsync(It.IsAny<Product>()), Times.Once);
        }
    }
}
