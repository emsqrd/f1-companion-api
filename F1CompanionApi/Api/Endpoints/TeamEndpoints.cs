using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

    private static async Task<IEnumerable<Team>> GetTeams(ApplicationDbContext db)
    {
        var teams = await db.Teams.ToListAsync();
        return teams;
    }

    private static async Task<Team> GetTeamByIdAsync(int id, ApplicationDbContext db)
    {
        var team = await db.Teams.Where(team => team.Id == id).FirstOrDefaultAsync();
        return team;
    }
}
