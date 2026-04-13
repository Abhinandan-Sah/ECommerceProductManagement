using Catalog.API.Application.DTOs.Product;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Enums;
using Catalog.API.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Catalog.API.Tests.Integration.Controllers
{
    [TestFixture]
    public class ProductsControllerIntegrationTests
    {
        private CustomWebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private IServiceScope _scope = null!;
        private CatalogDbContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
            _scope?.Dispose();
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task GetAllProducts_ShouldReturnPublishedProducts_WithHttp200OK()
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
                PublishStatus = PublishStatus.Published,
                CategoryId = category.Id
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var products = await response.Content.ReadFromJsonAsync<List<ProductResponseDto>>();
            Assert.That(products, Is.Not.Null);
            Assert.That(products, Has.Count.EqualTo(1));
            Assert.That(products![0].Name, Is.EqualTo("Laptop"));
            Assert.That(products[0].Brand, Is.EqualTo("Dell"));
            Assert.That(products[0].SKU, Is.EqualTo("DELL-001"));
        }

        [Test]
        public async Task GetProductById_ShouldReturnProduct_WithHttp200OK()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics"
            };
            await _context.Categories.AddAsync(category);

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Laptop",
                Brand = "Dell",
                Description = "High-performance laptop",
                SKU = "DELL-001",
                PublishStatus = PublishStatus.Published,
                CategoryId = category.Id
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/products/{productId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var productResponse = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
            Assert.That(productResponse, Is.Not.Null);
            Assert.That(productResponse!.Id, Is.EqualTo(productId));
            Assert.That(productResponse.Name, Is.EqualTo("Laptop"));
            Assert.That(productResponse.Brand, Is.EqualTo("Dell"));
            Assert.That(productResponse.Description, Is.EqualTo("High-performance laptop"));
            Assert.That(productResponse.SKU, Is.EqualTo("DELL-001"));
            Assert.That(productResponse.PublishStatus, Is.EqualTo(PublishStatus.Published));
            Assert.That(productResponse.CategoryName, Is.EqualTo("Electronics"));
        }
    }
}
