using F1CompanionApi.Api.Models;
using F1CompanionApi.Domain.Services;
using F1CompanionApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace F1CompanionApi.Api.Endpoints;

public static class MeEndpoints
{
    public record RegisterUserRequest(string? DisplayName);

    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me/profile", GetUserProfileAsync)
            .WithName("Get User Profile")
            .WithOpenApi()
            .WithDescription("Gets user profile")
            .RequireAuthorization();

        app.MapPost("/me/register", RegisterUserAsync)
            .WithName("Register User")
            .WithOpenApi()
            .WithDescription("Creates user account and profile")
            .RequireAuthorization();

        app.MapPatch("/me/profile", UpdateUserProfileAsync)
            .WithName("Update User Profile")
            .WithOpenApi()
            .WithDescription("Updates user profile")
            .RequireAuthorization();

        app.MapGet("/me/leagues", GetMyLeaguesAsync)
            .WithName("Get My Leagues")
            .WithOpenApi()
            .WithDescription("Gets leagues owned by the authenticated user")
            .RequireAuthorization();

        app.MapGet("/me/team", GetMyTeamAsync)
            .WithName("Get My Team")
            .WithOpenApi()
            .WithDescription("Get current user's team or null if none exists")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetUserProfileAsync(
        IUserProfileService userProfileService,
        [FromServices] ILogger logger)
    {
        logger.LogDebug("Fetching current user profile");
        var user = await userProfileService.GetCurrentUserProfileAsync();

        if (user is null)
        {
            logger.LogWarning("No user profile found for authenticated user");
            return Results.Problem(
                detail: "User profile not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Results.Ok(user);
    }

    private static async Task<IResult> RegisterUserAsync(
        HttpContext context,
        ISupabaseAuthService authService,
        IUserProfileService userProfileService,
        RegisterUserRequest request,
        [FromServices] ILogger logger
    )
    {
        var userId = authService.GetRequiredUserId();
        var userEmail = authService.GetUserEmail();

        logger.LogInformation("Registering user {UserId}", userId);

        if (userEmail is null)
        {
            logger.LogWarning("Registration attempted without email for user {UserId}", userId);
            return Results.Problem(
                detail: "Email address is required for registration",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var existingProfile = await userProfileService.GetUserProfileByAccountIdAsync(userId);
        if (existingProfile is not null)
        {
            logger.LogWarning("User {UserId} already registered", userId);
            return Results.Problem(
                detail: "User already registered",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        try
        {
            var userProfile = await userProfileService.CreateUserProfileAsync(
                userId,
                userEmail,
                request.DisplayName
            );

            logger.LogInformation("Successfully registered user {UserId} with profile {ProfileId}",
                userId, userProfile.Id);

            return Results.Created($"/me/profile", userProfile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register user {UserId}", userId);
            throw;
        }
    }

    private static async Task<IResult> UpdateUserProfileAsync(
        HttpContext httpContext,
        ISupabaseAuthService authService,
        IUserProfileService userProfileService,
        UpdateUserProfileRequest updateUserProfileRequest,
        [FromServices] ILogger logger
    )
    {
        logger.LogInformation("Updating user profile {ProfileId}", updateUserProfileRequest.Id);

        var existingProfile = await userProfileService.GetCurrentUserProfileAsync();
        if (existingProfile is null)
        {
            logger.LogWarning("User profile not found when attempting update");
            return Results.Problem(
                detail: "User profile not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        try
        {
            var updatedProfile = await userProfileService.UpdateUserProfileAsync(
                updateUserProfileRequest
            );

            logger.LogInformation("Successfully updated user profile {ProfileId}",
                updateUserProfileRequest.Id);

            return Results.Ok(updatedProfile);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "User profile {ProfileId} not found during update",
                updateUserProfileRequest.Id);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user profile {ProfileId}",
                updateUserProfileRequest.Id);
            throw;
        }
    }

    private static async Task<IResult> GetMyLeaguesAsync(
        IUserProfileService userProfileService,
        ILeagueService leagueService,
        [FromServices] ILogger logger
    )
    {
        logger.LogDebug("Fetching leagues for current user");

        try
        {
            var user = await userProfileService.GetRequiredCurrentUserProfileAsync();
            var leagues = await leagueService.GetLeaguesByOwnerIdAsync(user.Id);

            var leagueResponses = leagues.Select(league => new LeagueResponse
            {
                Id = league.Id,
                Name = league.Name,
                Description = league.Description,
                OwnerName = league.Owner.GetFullName(),
                MaxTeams = league.MaxTeams,
                IsPrivate = league.IsPrivate,
            }).ToList();

            logger.LogDebug("Retrieved {LeagueCount} leagues for current user", leagueResponses.Count);

            return Results.Ok(leagueResponses);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "User profile not found when fetching leagues");
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch leagues for current user");
            throw;
        }
    }

    private static async Task<IResult> GetMyTeamAsync(
        ITeamService teamService,
        IUserProfileService userProfileService,
        [FromServices] ILogger logger
    )
    {
        var user = await userProfileService.GetRequiredCurrentUserProfileAsync();

        logger.LogDebug("Fetching team for user {UserId}", user.Id);

        var team = await teamService.GetUserTeamAsync(user.Id);

        if (team is null)
        {
            logger.LogWarning("User {UserId} has no team", user.Id);
            return Results.Ok(null);
        }

        return Results.Ok(team);
    }
}
