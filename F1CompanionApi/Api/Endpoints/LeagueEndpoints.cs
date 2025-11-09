using F1CompanionApi.Api.Models;
using F1CompanionApi.Domain.Services;
using F1CompanionApi.Extensions;

namespace F1CompanionApi.Api.Endpoints;

public static class LeagueEndpoints
{
    public static IEndpointRouteBuilder MapLeagueEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/leagues", CreateLeagueAsync)
            .RequireAuthorization()
            .WithName("CreateLeague")
            .WithOpenApi()
            .WithDescription("Create a new League");

        app.MapGet("/leagues", GetLeaguesAsync)
            .RequireAuthorization()
            .WithName("GetLeagues")
            .WithOpenApi()
            .WithDescription("Gets all leagues");

        app.MapGet("/leagues/{id}", GetLeagueByIdAsync)
            .RequireAuthorization()
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
        var user = await userProfileService.GetRequiredCurrentUserProfileAsync();

        var leagueResponse = await leagueService.CreateLeagueAsync(createLeagueRequest, user.Id);

        return Results.Created($"/leagues/{leagueResponse.Id}", leagueResponse);

    }

    private static async Task<IResult> GetLeaguesAsync(ILeagueService leagueService)
    {
        var leagues = await leagueService.GetLeaguesAsync();

        var leagueResponses = leagues?.Select(league => new LeagueResponseModel
        {
            Id = league.Id,
            Name = league.Name,
            Description = league.Description,
            OwnerName = league.Owner.GetFullName(),
            MaxTeams = league.MaxTeams,
            IsPrivate = league.IsPrivate,
        }) ?? [];

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
            Description = league.Description,
            OwnerName = league.Owner.GetFullName(),
            MaxTeams = league.MaxTeams,
            IsPrivate = league.IsPrivate,
        };

        return Results.Ok(leagueResponse);
    }
}
