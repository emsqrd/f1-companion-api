using System;

namespace F1CompanionApi.Endpoints;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api")
        .MapTeamEndpoints()
        .MapMeEndpoints();

        return app;
    }
}
