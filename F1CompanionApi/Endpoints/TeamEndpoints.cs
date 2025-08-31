using System;
using Microsoft.AspNetCore.Http.HttpResults;

namespace F1CompanionApi.Endpoints;

public static class TeamEndpoints
{

    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/teams", GetTeams)
            .WithName("GetTeams")
            .WithOpenApi()
            .WithDescription("Gets all teams");

        return app;
    }

    private static Team[] GetTeams()
    {
        Team[] teams =
        [
            new Team
            {
                Id = 1,
                Name = "Team 1",
            },
            new Team
            {
                Id = 2,
                Name = "Team 2",
            }
        ];

        return teams;

    }
}

internal class Team
{
    public int Id { get; set; }
    public string? Name { get; set; }
}