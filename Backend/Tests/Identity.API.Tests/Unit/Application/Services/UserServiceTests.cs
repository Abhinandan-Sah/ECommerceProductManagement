using Identity.API.Application.DTOs.User;
using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Application.Services;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Identity.API.Tests.Identity.API.Tests.Unit.Application.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _mockUserRepo = null!;
    private Mock<ILogger<UserService>> _mockLogger = null!;
    private UserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _service = new UserService(_mockUserRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "User 1", Email = "user1@example.com", Role = Role.Customer },
            new User { Id = Guid.NewGuid(), FullName = "User 2", Email = "user2@example.com", Role = Role.Admin }
        };

        _mockUserRepo.Setup(r => r.GetAllUsersAsync(1, 10, null, null)).ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync(1, 10, null, null);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockUserRepo.Verify(r => r.GetAllUsersAsync(1, 10, null, null), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = "Test User",
            Email = "test@example.com",
            Role = Role.Customer
        };

        _mockUserRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(userId));
        Assert.That(result.FullName, Is.EqualTo("Test User"));
    }

    [Test]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateProfileAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = "Old Name",
            Email = "old@example.com",
            Role = Role.Customer
        };

        var request = new UpdateProfileRequestDto
        {
            FullName = "New Name",
            Email = "new@example.com"
        };

        _mockUserRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateProfileAsync(userId, request);

        // Assert
        Assert.That(result.FullName, Is.EqualTo("New Name"));
        Assert.That(result.Email, Is.EqualTo("new@example.com"));
        _mockUserRepo.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public void UpdateProfileAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateProfileRequestDto
        {
            FullName = "New Name",
            Email = "new@example.com"
        };

        _mockUserRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _service.UpdateProfileAsync(userId, request));
    }

    [Test]
    public async Task SetUserActiveAsync_WithValidId_UpdatesActiveStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = "Test User",
            Email = "test@example.com",
            Role = Role.Customer,
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.RevokeRefreshTokensAsync(userId, It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await _service.SetUserActiveAsync(userId, false);

        // Assert
        Assert.That(user.IsActive, Is.False);
        _mockUserRepo.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepo.Verify(r => r.RevokeRefreshTokensAsync(userId, "Account deactivated"), Times.Once);
    }

    [Test]
    public async Task DeleteUserAsync_WithValidId_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = "Test User",
            Email = "test@example.com",
            Role = Role.Customer
        };

        _mockUserRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.DeleteUserAsync(user)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteUserAsync(userId);

        // Assert
        _mockUserRepo.Verify(r => r.DeleteUserAsync(user), Times.Once);
    }
}
