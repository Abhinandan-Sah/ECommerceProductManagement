using System.Net;
using NUnit.Framework;

namespace Reporting.API.Tests.Integration.Controllers;

[TestFixture]
public class ReportsControllerIntegrationTests
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
    public async Task GetReports_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/reports");

        // Assert - Endpoint doesn't exist or returns NotFound
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
