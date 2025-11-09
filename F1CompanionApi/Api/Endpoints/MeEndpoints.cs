using F1CompanionApi.Api.Models;
using F1CompanionApi.Domain.Services;
using F1CompanionApi.Extensions;

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

        return app;
    }

    private static async Task<IResult> GetUserProfileAsync(IUserProfileService userProfileService)
    {
        var user = await userProfileService.GetCurrentUserProfileAsync();

        return Results.Ok(user);
    }

    private static async Task<IResult> RegisterUserAsync(
        HttpContext httpContext,
        ISupabaseAuthService authService,
        IUserProfileService userProfileService,
        RegisterUserRequest request
    )
    {
        var userId = authService.GetRequiredUserId();
        var userEmail = authService.GetUserEmail();

        if (userEmail is null)
        {
            return Results.BadRequest("Email address is required for registration");
        }

        var existingProfile = await userProfileService.GetUserProfileByAccountIdAsync(userId);
        if (existingProfile is not null)
        {
            return Results.Conflict("User already registered");
        }

        var userProfile = await userProfileService.CreateUserProfileAsync(
            userId,
            userEmail,
            request.DisplayName
        );

        return Results.Created($"/me/profile", userProfile);
    }

    private static async Task<IResult> UpdateUserProfileAsync(
        HttpContext httpContext,
        ISupabaseAuthService authService,
        IUserProfileService userProfileService,
        UpdateUserProfileRequest updateUserProfileRequest
    )
    {
        var existingProfile = await userProfileService.GetCurrentUserProfileAsync();
        if (existingProfile is null)
        {
            return Results.NotFound("User profile not found");
        }

        var updatedProfile = await userProfileService.UpdateUserProfileAsync(
            updateUserProfileRequest
        );

        return Results.Ok(updatedProfile);
    }

    private static async Task<IResult> GetMyLeaguesAsync(
        IUserProfileService userProfileService,
        ILeagueService leagueService
    )
    {
        var user = await userProfileService.GetRequiredCurrentUserProfileAsync();

        var leagues = await leagueService.GetLeaguesByOwnerIdAsync(user.Id);

        var leagueResponses = leagues.Select(league => new LeagueResponseModel
        {
            Id = league.Id,
            Name = league.Name,
            Description = league.Description,
            OwnerName = league.Owner.GetFullName(),
            MaxTeams = league.MaxTeams,
            IsPrivate = league.IsPrivate,
        });

        return Results.Ok(leagueResponses);
    }
}
