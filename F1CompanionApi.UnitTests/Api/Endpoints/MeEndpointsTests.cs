using F1CompanionApi.Api.Endpoints;
using F1CompanionApi.Api.Models;
using F1CompanionApi.Data.Entities;
using F1CompanionApi.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace F1CompanionApi.UnitTests.Api.Endpoints;

public class MeEndpointsTests
{
    private readonly Mock<ISupabaseAuthService> _mockAuthService;
    private readonly Mock<IUserProfileService> _mockUserProfileService;
    private readonly Mock<ILeagueService> _mockLeagueService;
    private readonly Mock<HttpContext> _mockHttpContext;

    public MeEndpointsTests()
    {
        _mockAuthService = new Mock<ISupabaseAuthService>();
        _mockUserProfileService = new Mock<IUserProfileService>();
        _mockLeagueService = new Mock<ILeagueService>();
        _mockHttpContext = new Mock<HttpContext>();
    }

    [Theory]
    [InlineData("Test User")]
    [InlineData(null)]
    public async Task RegisterUserAsync_ValidRequest_ReturnsCreatedWithProfile(string? displayName)
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@test.com";

        var request = new MeEndpoints.RegisterUserRequest(displayName);

        var createdProfile = new UserProfile
        {
            Id = 1,
            AccountId = userId,
            Email = userEmail,
            DisplayName = displayName
        };

        _mockAuthService
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        _mockAuthService
            .Setup(x => x.GetUserEmail())
            .Returns(userEmail);

        _mockUserProfileService
            .Setup(x => x.GetUserProfileByAccountIdAsync(userId))
            .ReturnsAsync((UserProfile?)null);

        _mockUserProfileService
            .Setup(x => x.CreateUserProfileAsync(userId, userEmail, displayName))
            .ReturnsAsync(createdProfile);

        // Act
        var result = await InvokeRegisterUserAsync(request);

        // Assert
        Assert.IsType<Created<UserProfile>>(result);
        var createdResult = (Created<UserProfile>)result;
        Assert.Equal("/me/profile", createdResult.Location);
        Assert.Equal(createdProfile, createdResult.Value);
    }

    [Fact]
    public async Task RegisterUserAsync_UserEmailIsNull_ReturnsBadRequest()
    {
        // Arrange
        var userId = "test-user-id";
        var request = new MeEndpoints.RegisterUserRequest("Test User");

        _mockAuthService
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        _mockAuthService
            .Setup(x => x.GetUserEmail())
            .Returns((string?)null);

        // Act
        var result = await InvokeRegisterUserAsync(request);

        // Assert
        Assert.IsType<BadRequest<string>>(result);
        var badRequestResult = (BadRequest<string>)result;
        Assert.Equal("Email address is required for registration", badRequestResult.Value);
    }

    [Fact]
    public async Task RegisterUserAsync_UserAlreadyRegistered_ReturnsConflict()
    {
        // Arrange
        var userId = "test-user-id";
        var userEmail = "test@test.com";
        var request = new MeEndpoints.RegisterUserRequest("Test User");

        var existingProfile = new UserProfile
        {
            Id = 1,
            AccountId = userId,
            Email = userEmail
        };

        _mockAuthService
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        _mockAuthService
            .Setup(x => x.GetUserEmail())
            .Returns(userEmail);

        _mockUserProfileService
            .Setup(x => x.GetUserProfileByAccountIdAsync(userId))
            .ReturnsAsync(existingProfile);

        // Act
        var result = await InvokeRegisterUserAsync(request);

        // Assert
        Assert.IsType<Conflict<string>>(result);
        var conflictResult = (Conflict<string>)result;
        Assert.Equal("User already registered", conflictResult.Value);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_ValidRequest_ReturnsOkWithUpdatedProfile()
    {
        // Arrange
        var existingProfile = new UserProfile
        {
            Id = 1,
            AccountId = "test-account",
            Email = "test@test.com",
            DisplayName = "Old Name"
        };

        var updateRequest = new UpdateUserProfileRequest
        {
            Id = 1,
            DisplayName = "New Name",
            FirstName = "John",
            LastName = "Doe"
        };

        var updatedResponse = new UserProfileResponse
        {
            Id = 1,
            Email = "test@test.com",
            DisplayName = "New Name",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserProfileService
            .Setup(x => x.GetCurrentUserProfileAsync())
            .ReturnsAsync(existingProfile);

        _mockUserProfileService
            .Setup(x => x.UpdateUserProfileAsync(updateRequest))
            .ReturnsAsync(updatedResponse);

        // Act
        var result = await InvokeUpdateUserProfileAsync(updateRequest);

        // Assert
        Assert.IsType<Ok<UserProfileResponse>>(result);
        var okResult = (Ok<UserProfileResponse>)result;
        Assert.Equal(updatedResponse, okResult.Value);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_UserProfileNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateUserProfileRequest
        {
            Id = 1,
            DisplayName = "New Name"
        };

        _mockUserProfileService
            .Setup(x => x.GetCurrentUserProfileAsync())
            .ReturnsAsync((UserProfile?)null);

        // Act
        var result = await InvokeUpdateUserProfileAsync(updateRequest);

        // Assert
        Assert.IsType<NotFound<string>>(result);
        var notFoundResult = (NotFound<string>)result;
        Assert.Equal("User profile not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetMyLeaguesAsync_UserHasLeagues_ReturnsOkWithLeagues()
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

        var leagues = new List<League>
        {
            new League
            {
                Id = 1,
                Name = "League 1",
                Description = "Description 1",
                OwnerId = userProfile.Id,
                Owner = userProfile,
                MaxTeams = 15,
                IsPrivate = true,
                CreatedBy = userProfile.Id,
                CreatedAt = DateTime.UtcNow
            },
            new League
            {
                Id = 2,
                Name = "League 2",
                Description = "Description 2",
                OwnerId = userProfile.Id,
                Owner = userProfile,
                MaxTeams = 20,
                IsPrivate = false,
                CreatedBy = userProfile.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockUserProfileService
            .Setup(x => x.GetRequiredCurrentUserProfileAsync())
            .ReturnsAsync(userProfile);

        _mockLeagueService
            .Setup(x => x.GetLeaguesByOwnerIdAsync(userProfile.Id))
            .ReturnsAsync(leagues);

        // Act
        var result = await InvokeGetMyLeaguesAsync();

        // Assert
        Assert.IsType<Ok<IEnumerable<LeagueResponseModel>>>(result);
        var okResult = (Ok<IEnumerable<LeagueResponseModel>>)result;
        Assert.NotNull(okResult.Value);
        var leagueList = okResult.Value.ToList();
        Assert.Equal(2, leagueList.Count);
        Assert.Equal("League 1", leagueList[0].Name);
        Assert.Equal("John Doe", leagueList[0].OwnerName);
        Assert.Equal("League 2", leagueList[1].Name);
        Assert.Equal(20, leagueList[1].MaxTeams);
    }

    [Fact]
    public async Task GetMyLeaguesAsync_UserHasNoLeagues_ReturnsOkWithEmptyCollection()
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

        _mockUserProfileService
            .Setup(x => x.GetRequiredCurrentUserProfileAsync())
            .ReturnsAsync(userProfile);

        _mockLeagueService
            .Setup(x => x.GetLeaguesByOwnerIdAsync(userProfile.Id))
            .ReturnsAsync(new List<League>());

        // Act
        var result = await InvokeGetMyLeaguesAsync();

        // Assert
        Assert.IsType<Ok<IEnumerable<LeagueResponseModel>>>(result);
        var okResult = (Ok<IEnumerable<LeagueResponseModel>>)result;
        Assert.NotNull(okResult.Value);
        Assert.Empty(okResult.Value);
    }

    [Fact]
    public async Task GetMyLeaguesAsync_ServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockUserProfileService
            .Setup(x => x.GetRequiredCurrentUserProfileAsync())
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => InvokeGetMyLeaguesAsync()
        );
    }

    // Helper methods to invoke private endpoint methods via reflection
    private async Task<IResult> InvokeRegisterUserAsync(MeEndpoints.RegisterUserRequest request)
    {
        var method = typeof(MeEndpoints).GetMethod(
            "RegisterUserAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object[]
            {
                _mockHttpContext.Object,
                _mockAuthService.Object,
                _mockUserProfileService.Object,
                request
            }
        )!;

        return await task;
    }

    private async Task<IResult> InvokeUpdateUserProfileAsync(
        UpdateUserProfileRequest updateRequest
    )
    {
        var method = typeof(MeEndpoints).GetMethod(
            "UpdateUserProfileAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object[]
            {
                _mockHttpContext.Object,
                _mockAuthService.Object,
                _mockUserProfileService.Object,
                updateRequest
            }
        )!;

        return await task;
    }

    private async Task<IResult> InvokeGetMyLeaguesAsync()
    {
        var method = typeof(MeEndpoints).GetMethod(
            "GetMyLeaguesAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object[]
            {
                _mockUserProfileService.Object,
                _mockLeagueService.Object
            }
        )!;

        return await task;
    }
}
