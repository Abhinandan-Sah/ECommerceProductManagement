using System.Net;
using System.Net.Http.Json;
using Catalog.API.Application.DTOs.MediaAsset;
using NUnit.Framework;

namespace Catalog.API.Tests.Integration.Controllers;

[TestFixture]
public class MediaAssetsControllerIntegrationTests
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
    public async Task GetMediaByProduct_ReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}/media");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var mediaAssets = await response.Content.ReadFromJsonAsync<List<MediaAssetResponseDto>>();
        Assert.That(mediaAssets, Is.Not.Null);
    }
}
