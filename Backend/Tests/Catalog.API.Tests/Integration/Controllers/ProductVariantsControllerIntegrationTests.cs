using System.Net;
using System.Net.Http.Json;
using Catalog.API.Application.DTOs.ProductVariant;
using NUnit.Framework;

namespace Catalog.API.Tests.Integration.Controllers;

[TestFixture]
public class ProductVariantsControllerIntegrationTests
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
    public async Task GetVariantsByProduct_ReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}/variants");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var variants = await response.Content.ReadFromJsonAsync<List<ProductVariantResponseDto>>();
        Assert.That(variants, Is.Not.Null);
    }
}
