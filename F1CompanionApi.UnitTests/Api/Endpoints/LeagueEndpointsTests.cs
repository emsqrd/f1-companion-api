using F1CompanionApi.Api.Endpoints;
using F1CompanionApi.Api.Models;
using F1CompanionApi.Data.Entities;
using F1CompanionApi.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace F1CompanionApi.UnitTests.Api.Endpoints;

public class LeagueEndpointsTests
{
    private readonly Mock<ISupabaseAuthService> _mockAuthService;
    private readonly Mock<IUserProfileService> _mockUserProfileService;
    private readonly Mock<ILeagueService> _mockLeagueService;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<ILogger> _mockLogger;

    public LeagueEndpointsTests()
    {
        _mockAuthService = new Mock<ISupabaseAuthService>();
        _mockUserProfileService = new Mock<IUserProfileService>();
        _mockLeagueService = new Mock<ILeagueService>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public async Task CreateLeagueAsync_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            Id = 1,
            AccountId = "test-account",
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var request = new CreateLeagueRequest
        {
            Name = "Test League",
            Description = "Test Description"
        };

        var expectedResponse = new LeagueResponseModel
        {
            Id = 1,
            Name = "Test League",
            Description = "Test Description",
            OwnerName = "John Doe",
            MaxTeams = 15,
            IsPrivate = true
        };

        _mockUserProfileService
            .Setup(x => x.GetRequiredCurrentUserProfileAsync())
            .ReturnsAsync(userProfile);

        _mockLeagueService
            .Setup(x => x.CreateLeagueAsync(request, userProfile.Id))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await InvokeCreateLeagueAsync(request);

        // Assert
        Assert.IsType<Created<LeagueResponseModel>>(result);
        var createdResult = (Created<LeagueResponseModel>)result;
        Assert.Equal("/leagues/1", createdResult.Location);
        Assert.Equal(expectedResponse, createdResult.Value);
    }

    [Fact]
    public async Task CreateLeagueAsync_UserProfileServiceThrows_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "Test League"
        };

        _mockUserProfileService
            .Setup(x => x.GetRequiredCurrentUserProfileAsync())
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var result = await InvokeCreateLeagueAsync(request);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetLeaguesAsync_LeaguesExist_ReturnsOkWithLeagues()
    {
        // Arrange
        var owner = new UserProfile
        {
            Id = 1,
            AccountId = "test-account",
            Email = "owner@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var leagues = new List<League>
        {
            new League
            {
                Id = 1,
                Name = "League 1",
                Description = "Description 1",
                OwnerId = owner.Id,
                Owner = owner,
                MaxTeams = 15,
                IsPrivate = true,
                CreatedBy = owner.Id,
                CreatedAt = DateTime.UtcNow
            },
            new League
            {
                Id = 2,
                Name = "League 2",
                OwnerId = owner.Id,
                Owner = owner,
                MaxTeams = 20,
                IsPrivate = false,
                CreatedBy = owner.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockLeagueService
            .Setup(x => x.GetLeaguesAsync())
            .ReturnsAsync(leagues);

        // Act
        var result = await InvokeGetLeaguesAsync();

        // Assert
        Assert.IsType<Ok<IEnumerable<LeagueResponseModel>>>(result);
        var okResult = (Ok<IEnumerable<LeagueResponseModel>>)result;
        Assert.NotNull(okResult.Value);
        var leagueList = okResult.Value.ToList();
        Assert.Equal(2, leagueList.Count);
        Assert.Equal("League 1", leagueList[0].Name);
        Assert.Equal("League 2", leagueList[1].Name);
    }

    [Fact]
    public async Task GetLeaguesAsync_NoLeagues_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        _mockLeagueService
            .Setup(x => x.GetLeaguesAsync())
            .ReturnsAsync(new List<League>());

        // Act
        var result = await InvokeGetLeaguesAsync();

        // Assert
        Assert.IsType<Ok<IEnumerable<LeagueResponseModel>>>(result);
        var okResult = (Ok<IEnumerable<LeagueResponseModel>>)result;
        Assert.NotNull(okResult.Value);
        Assert.Empty(okResult.Value);
    }

    [Fact]
    public async Task GetLeaguesAsync_ServiceReturnsNull_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        _mockLeagueService
            .Setup(x => x.GetLeaguesAsync())
            .ReturnsAsync((IEnumerable<League>?)null!);

        // Act
        var result = await InvokeGetLeaguesAsync();

        // Assert
        Assert.IsType<Ok<IEnumerable<LeagueResponseModel>>>(result);
        var okResult = (Ok<IEnumerable<LeagueResponseModel>>)result;
        Assert.NotNull(okResult.Value);
        Assert.Empty(okResult.Value);
    }

    [Fact]
    public async Task GetLeagueByIdAsync_LeagueExists_ReturnsOkWithLeague()
    {
        // Arrange
        var owner = new UserProfile
        {
            Id = 1,
            AccountId = "test-account",
            Email = "owner@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var league = new League
        {
            Id = 1,
            Name = "Test League",
            Description = "Test Description",
            OwnerId = owner.Id,
            Owner = owner,
            MaxTeams = 15,
            IsPrivate = true,
            CreatedBy = owner.Id,
            CreatedAt = DateTime.UtcNow
        };

        _mockLeagueService
            .Setup(x => x.GetLeagueByIdAsync(1))
            .ReturnsAsync(league);

        // Act
        var result = await InvokeGetLeagueByIdAsync(1);

        // Assert
        Assert.IsType<Ok<LeagueResponseModel>>(result);
        var okResult = (Ok<LeagueResponseModel>)result;
        Assert.NotNull(okResult.Value);
        Assert.Equal(1, okResult.Value.Id);
        Assert.Equal("Test League", okResult.Value.Name);
        Assert.Equal("John Doe", okResult.Value.OwnerName);
    }

    [Fact]
    public async Task GetLeagueByIdAsync_LeagueDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockLeagueService
            .Setup(x => x.GetLeagueByIdAsync(999))
            .ReturnsAsync((League?)null);

        // Act
        var result = await InvokeGetLeagueByIdAsync(999);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    // Helper methods to invoke private endpoint methods via reflection
    private async Task<IResult> InvokeCreateLeagueAsync(CreateLeagueRequest request)
    {
        var method = typeof(LeagueEndpoints).GetMethod(
            "CreateLeagueAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object[]
            {
                _mockHttpContext.Object,
                _mockAuthService.Object,
                _mockUserProfileService.Object,
                _mockLeagueService.Object,
                request,
                _mockLogger.Object
            }
        )!;

        return await task;
    }

    private async Task<IResult> InvokeGetLeaguesAsync()
    {
        var method = typeof(LeagueEndpoints).GetMethod(
            "GetLeaguesAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object[] { _mockLeagueService.Object, _mockLogger.Object }
        )!;

        return await task;
    }

    private async Task<IResult> InvokeGetLeagueByIdAsync(int id)
    {
        var method = typeof(LeagueEndpoints).GetMethod(
            "GetLeagueByIdAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object[] { _mockLeagueService.Object, id, _mockLogger.Object }
        )!;

        return await task;
    }
}
