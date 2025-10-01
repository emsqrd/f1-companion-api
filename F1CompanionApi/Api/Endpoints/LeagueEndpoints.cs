using F1CompanionApi.Api.Models;
using F1CompanionApi.Data.Models;
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

        return app;
    }

    //TODO: Scope this down to only get leagues for the authenticated user
    private async static Task<IEnumerable<LeagueResponseModel>> GetLeaguesAsync(ILeagueService leagueService)
    {
        var leagues = await leagueService.GetLeaguesAsync();

        return leagues.Select(league => new LeagueResponseModel
        {
            Id = league.Id,
            Name = league.Name,
        });

    }
}
