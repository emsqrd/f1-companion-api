using F1CompanionApi.Api.Models;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using F1CompanionApi.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace F1CompanionApi.UnitTests.Services;

public class TeamServiceTests
{
    private readonly Mock<ILogger<TeamService>> _mockLogger;

    public TeamServiceTests()
    {
        _mockLogger = new Mock<ILogger<TeamService>>();
    }

    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateTeamAsync_ValidRequest_ReturnsTeamResponseWithCorrectData()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var user = new UserProfile
        {
            AccountId = "test-account",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        context.UserProfiles.Add(user);
        await context.SaveChangesAsync();

        var request = new CreateTeamRequest
        {
            Name = "Test Team"
        };

        // Act
        var result = await service.CreateTeamAsync(request, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Team", result.Name);
        Assert.Equal("John Doe", result.OwnerName);
    }

    [Fact]
    public async Task CreateTeamAsync_ValidRequest_PersistsTeamToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var user = new UserProfile
        {
            AccountId = "test-account",
            Email = "user@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };
        context.UserProfiles.Add(user);
        await context.SaveChangesAsync();

        var request = new CreateTeamRequest
        {
            Name = "Persistent Team"
        };

        // Act
        await service.CreateTeamAsync(request, user.Id);

        // Assert
        var savedTeam = await context.Teams.FirstOrDefaultAsync();
        Assert.NotNull(savedTeam);
        Assert.Equal("Persistent Team", savedTeam.Name);
        Assert.Equal(user.Id, savedTeam.UserId);
        Assert.Equal(user.Id, savedTeam.CreatedBy);
    }

    [Fact]
    public async Task CreateTeamAsync_UserAlreadyHasTeam_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var user = new UserProfile
        {
            AccountId = "test-account",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        context.UserProfiles.Add(user);
        await context.SaveChangesAsync();

        var existingTeam = new Team
        {
            Name = "Existing Team",
            UserId = user.Id,
            CreatedBy = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Teams.Add(existingTeam);
        await context.SaveChangesAsync();

        var request = new CreateTeamRequest
        {
            Name = "New Team"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateTeamAsync(request, user.Id)
        );
        Assert.Equal("User already has a team", exception.Message);
    }

    [Fact]
    public async Task CreateTeamAsync_NonExistentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var request = new CreateTeamRequest
        {
            Name = "Test Team"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateTeamAsync(request, 999)
        );
        Assert.Equal("User not found", exception.Message);
    }

    [Theory]
    [InlineData("  Team With Spaces  ", "Team With Spaces")]
    [InlineData("   Leading Spaces", "Leading Spaces")]
    [InlineData("Trailing Spaces   ", "Trailing Spaces")]
    public async Task CreateTeamAsync_TeamNameWithWhitespace_TrimsWhitespace(
        string inputName,
        string expectedName
    )
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var user = new UserProfile
        {
            AccountId = "test-account",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        context.UserProfiles.Add(user);
        await context.SaveChangesAsync();

        var request = new CreateTeamRequest
        {
            Name = inputName
        };

        // Act
        var result = await service.CreateTeamAsync(request, user.Id);

        // Assert
        Assert.Equal(expectedName, result.Name);

        var savedTeam = await context.Teams.FirstOrDefaultAsync();
        Assert.NotNull(savedTeam);
        Assert.Equal(expectedName, savedTeam.Name);
    }

    [Fact]
    public async Task GetUserTeamAsync_UserHasTeam_ReturnsTeamResponse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var user = new UserProfile
        {
            AccountId = "test-account",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        context.UserProfiles.Add(user);
        await context.SaveChangesAsync();

        var team = new Team
        {
            Name = "Findable Team",
            UserId = user.Id,
            CreatedBy = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserTeamAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(team.Id, result.Id);
        Assert.Equal("Findable Team", result.Name);
        Assert.Equal("John Doe", result.OwnerName);
    }

    [Fact]
    public async Task GetUserTeamAsync_UserHasNoTeam_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var user = new UserProfile
        {
            AccountId = "test-account",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        context.UserProfiles.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserTeamAsync(user.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserTeamAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        // Act
        var result = await service.GetUserTeamAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTeamAsync_ConcurrentRequests_OnlyFirstSucceeds()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new TeamService(context, _mockLogger.Object);

        var user = new UserProfile
        {
            AccountId = "test-account",
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        context.UserProfiles.Add(user);
        await context.SaveChangesAsync();

        var request1 = new CreateTeamRequest { Name = "Team 1" };
        var request2 = new CreateTeamRequest { Name = "Team 2" };

        // Act - first request succeeds
        var result = await service.CreateTeamAsync(request1, user.Id);

        // Act & Assert - second request fails
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateTeamAsync(request2, user.Id)
        );

        Assert.Equal("User already has a team", exception.Message);
        Assert.Equal("Team 1", result.Name);

        // Verify only one team exists
        var teamCount = await context.Teams.CountAsync(t => t.UserId == user.Id);
        Assert.Equal(1, teamCount);
    }
}
