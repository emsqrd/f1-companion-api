using System;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Endpoints;

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

        return app;
    }

    private async static Task<IEnumerable<Team>> GetTeams(ApplicationDbContext db)
    {
        var teams = await db.Teams.ToListAsync();
        return teams;
    }

    private async static Task<Team> GetTeamByIdAsync(int id, ApplicationDbContext db)
    {
        var team = await db.Teams.Where(team => team.Id == id).FirstOrDefaultAsync();
        return team;
    }
}
