using F1CompanionApi.Api.Models;
using F1CompanionApi.Domain.Services;

namespace F1CompanionApi.Api.Endpoints;

public static class LeagueEndpoints
{
    public static IEndpointRouteBuilder MapLeagueEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/leagues", CreateLeagueAsync)
            .WithName("CreateLeague")
            .WithOpenApi()
            .WithDescription("Create a new League");

        app.MapGet("/leagues", GetLeaguesAsync)
            .WithName("GetLeagues")
            .WithOpenApi()
            .WithDescription("Gets all leagues");

        app.MapGet("/leagues/{id}", GetLeagueByIdAsync)
            .WithName("GetLeaguesById")
            .WithOpenApi()
            .WithDescription("Get League By Id");

        return app;
    }

    private static async Task<IResult> CreateLeagueAsync(
        HttpContext context,
        ISupabaseAuthService authService,
        IUserProfileService userProfileService,
        ILeagueService leagueService,
        CreateLeagueRequest createLeagueRequest
    )
    {
        var userId = authService.GetUserId(context.User);

        var user = await userProfileService.GetUserProfileByAccountIdAsync(userId);

        var leagueResponse = await leagueService.CreateLeagueAsync(createLeagueRequest, user.Id);

        return Results.Created($"/leagues/{leagueResponse.Id}", leagueResponse);
    }

    //TODO: Scope this down to only get leagues for the authenticated user
    private async static Task<IResult> GetLeaguesAsync(ILeagueService leagueService)
    {
        var leagues = await leagueService.GetLeaguesAsync();

        if (leagues is null)
        {
            return Results.NotFound("Leagues not found");
        }

        var leagueResponses = leagues.Select(league => new LeagueResponseModel
        {
            Id = league.Id,
            Name = league.Name,
            OwnerName = league.Owner.FullName,
            MaxTeams = league.MaxTeams,
            IsPrivate = league.IsPrivate,
        });

        return Results.Ok(leagueResponses);
    }

    private static async Task<IResult> GetLeagueByIdAsync(ILeagueService leagueService, int id)
    {
        var league = await leagueService.GetLeagueByIdAsync(id);

        if (league is null)
        {
            return Results.NotFound("League not found");
        }

        var leagueResponse = new LeagueResponseModel
        {
            Id = league.Id,
            Name = league.Name,
            OwnerName = league.Owner.FullName,
            MaxTeams = league.MaxTeams,
            IsPrivate = league.IsPrivate,
        };

        return Results.Ok(leagueResponse);
    }
}
