using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace F1CompanionApi.Api.Endpoints;

public static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/teams", GetTeams)
            .WithName("GetTeams")
            .WithOpenApi()
            .WithDescription("Gets all teams");

        app.MapGet("/teams/{id}", GetTeamByIdAsync)
            .WithName("GetTeamById")
            .WithOpenApi()
            .WithDescription("Get Team By Id");

        //TODO: Add new endpoint for leaderboard that orders teams by TotalPoints and assigns Rank on the fly

        return app;
    }

    private static async Task<IEnumerable<Team>> GetTeams(
        ApplicationDbContext db,
        [FromServices] ILogger logger)
    {
        logger.LogDebug("Fetching all teams");
        var teams = await db.Teams.ToListAsync() ?? [];
        logger.LogDebug("Retrieved {TeamCount} teams", teams.Count);
        return teams;
    }

    private static async Task<IResult> GetTeamByIdAsync(
        int id,
        ApplicationDbContext db,
        [FromServices] ILogger logger)
    {
        logger.LogDebug("Fetching team {TeamId}", id);
        var team = await db.Teams.Where(team => team.Id == id).FirstOrDefaultAsync();

        if (team is null)
        {
            logger.LogWarning("Team {TeamId} not found", id);
            return Results.Problem(
                detail: "Team not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Results.Ok(team);
    }
}
