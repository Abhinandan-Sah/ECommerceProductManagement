using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Infrastructure.Data;
using Identity.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Identity.API.Tests.Integration.Repositories;

[TestFixture]
public class AuthRepositoryIntegrationTests
{
    private IdentityDBContext _context = null!;
    private AuthRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<IdentityDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDBContext(options);
        _repository = new AuthRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = Role.Customer
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("test@example.com");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task EmailExistsAsync_WithNonExistingEmail_ReturnsFalse()
    {
        // Act
        var result = await _repository.EmailExistsAsync("nonexistent@example.com");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AddUserAsync_AddsUserToDatabase()
    {
        // Arrange
        var user = new User
        {
            FullName = "New User",
            Email = "new@example.com",
            PasswordHash = "hashedpassword",
            Role = Role.Customer
        };

        // Act
        await _repository.AddUserAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "new@example.com");
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser!.FullName, Is.EqualTo("New User"));
    }

    [Test]
    public async Task GetUserByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = Role.Customer
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public async Task GetUserByEmailAsync_WithNonExistingEmail_ReturnsNull()
    {
        // Act
        var result = await _repository.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.That(result, Is.Null);
    }
}
