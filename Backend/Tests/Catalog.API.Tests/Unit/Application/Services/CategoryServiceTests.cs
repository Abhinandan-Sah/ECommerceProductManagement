using Catalog.API.Application.DTOs.Category;
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
    [Category("Catalog")]
    [Category("Unit")]
    [Category("CategoryService")]
    [Author("Catalog.API Team")]
    public class CategoryServiceTests
    {
        private Mock<ICategoryRepository> _mockCategoryRepository = null!;
        private Mock<ILogger<CategoryService>> _mockLogger = null!;
        private CategoryService _categoryService = null!;

        [SetUp]
        public void SetUp()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockLogger = new Mock<ILogger<CategoryService>>();
            
            _categoryService = new CategoryService(
                _mockCategoryRepository.Object,
                _mockLogger.Object
            );
        }

        [Test]
        [Description("Verifies that category navigation can load the full catalog category list.")]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Category 1", ParentCategoryId = null },
                new Category { Id = Guid.NewGuid(), Name = "Category 2", ParentCategoryId = null }
            };

            _mockCategoryRepository
                .Setup(r => r.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        [Description("Verifies that a known category id returns the category details required by catalog workflows.")]
        public async Task GetCategoryByIdAsync_ExistingCategory_ReturnsCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                ParentCategoryId = null
            };

            _mockCategoryRepository
                .Setup(r => r.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(categoryId));
            Assert.That(result.Name, Is.EqualTo("Test Category"));
        }

        [Test]
        [Description("Verifies that missing categories are returned as null for optional lookup flows.")]
        public async Task GetCategoryByIdAsync_NonExistentCategory_ReturnsNull()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _mockCategoryRepository
                .Setup(r => r.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Description("Verifies that valid category input creates a new top-level catalog category.")]
        public async Task AddCategoryAsync_ValidData_CreatesCategory()
        {
            // Arrange
            var createDto = new CreateCategoryDto
            {
                Name = "New Category",
                ParentCategoryId = null
            };

            var savedCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                ParentCategoryId = createDto.ParentCategoryId
            };

            _mockCategoryRepository
                .Setup(r => r.AddCategoryAsync(It.IsAny<Category>()))
                .ReturnsAsync(savedCategory);

            // Act
            var result = await _categoryService.AddCategoryAsync(createDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Category"));
            Assert.That(result.ParentCategoryId, Is.Null);

            _mockCategoryRepository.Verify(r => r.AddCategoryAsync(It.IsAny<Category>()), Times.Once);
        }

        [Test]
        [Description("Verifies that updating a deleted or unknown category fails with a not-found error.")]
        public async Task UpdateCategoryAsync_NonExistentCategory_ThrowsNotFoundException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryUpdate = new Category { Name = "Updated Name" };

            _mockCategoryRepository
                .Setup(r => r.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotFoundException>(
                async () => await _categoryService.UpdateCategoryAsync(categoryId, categoryUpdate)
            );
            Assert.That(exception!.Message, Does.Contain("Category"));
        }

        [Test]
        [Description("Verifies that deleting a deleted or unknown category fails with a not-found error.")]
        public async Task DeleteCategoryAsync_NonExistentCategory_ThrowsNotFoundException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _mockCategoryRepository
                .Setup(r => r.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotFoundException>(
                async () => await _categoryService.DeleteCategoryAsync(categoryId)
            );
            Assert.That(exception!.Message, Does.Contain("Category"));
        }
    }
}
