using System.Net;
using System.Net.Http.Json;
using Catalog.API.Application.DTOs.Category;
using NUnit.Framework;

namespace Catalog.API.Tests.Integration.Controllers;

[TestFixture]
public class CategoriesControllerIntegrationTests
{
    private CustomWebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetAllCategories_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponseDto>>();
        Assert.That(categories, Is.Not.Null);
    }

    [Test]
    public async Task GetCategoryById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/categories/{invalidId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
