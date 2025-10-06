using F1CompanionApi.Api.Models;
using F1CompanionApi.Domain.Services;

namespace F1CompanionApi.Api.Endpoints;

public static class LeagueEndpoints
{
    public static IEndpointRouteBuilder MapLeagueEndpoints(this IEndpointRouteBuilder app)
    {
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

        var leagueResponse = new LeagueResponseModel { Id = league.Id, Name = league.Name };

        return Results.Ok(leagueResponse);
    }
}
