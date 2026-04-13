using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Infrastructure.Data;
using Identity.API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Identity.API.Tests.Integration.Repositories;

[TestFixture]
public class UserRepositoryIntegrationTests
{
    private IdentityDBContext _context = null!;
    private UserRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<IdentityDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDBContext(options);
        _repository = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        _context.Users.AddRange(
            new User { FullName = "User 1", Email = "user1@example.com", PasswordHash = "hash", Role = Role.Customer },
            new User { FullName = "User 2", Email = "user2@example.com", PasswordHash = "hash", Role = Role.Admin }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllUsersAsync(1, 10, null, null);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = Role.Customer
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByIdAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateUserAsync_UpdatesUser()
    {
        // Arrange
        var user = new User
        {
            FullName = "Old Name",
            Email = "old@example.com",
            PasswordHash = "hash",
            Role = Role.Customer
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.FullName = "New Name";

        // Act
        await _repository.UpdateUserAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.That(updatedUser!.FullName, Is.EqualTo("New Name"));
    }

    [Test]
    public async Task DeleteUserAsync_DeletesUser()
    {
        // Arrange
        var user = new User
        {
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = Role.Customer
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteUserAsync(user);

        // Assert
        var deletedUser = await _context.Users.FindAsync(user.Id);
        Assert.That(deletedUser, Is.Null);
    }
}
