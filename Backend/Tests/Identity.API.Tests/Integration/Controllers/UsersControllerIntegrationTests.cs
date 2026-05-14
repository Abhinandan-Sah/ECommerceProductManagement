using System.Net;
using NUnit.Framework;

namespace Identity.API.Tests.Integration.Controllers;

[TestFixture]
[Category("Identity")]
[Category("Integration")]
[Category("UsersController")]
[Author("Identity.API Team")]
public class UsersControllerIntegrationTests
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
    [Description("Verifies that user profile endpoints reject unauthenticated access before exposing account data.")]
    public async Task GetUserById_WithInvalidId_ReturnsUnauthorized()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/users/{invalidId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
