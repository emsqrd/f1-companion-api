using System;
using F1CompanionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace F1CompanionApi.Endpoints;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", GetProfile)
            .WithName("GetMe")
            .WithOpenApi()
            .WithDescription("Gets user profile")
            .RequireAuthorization();

        return app;
    }

    private static IResult GetProfile(HttpContext context, SupabaseAuthService authService)
    {
        var userId = authService.GetUserId(context.User);
        var email = context.User.FindFirst("email")?.Value;

        return Results.Ok(new { UserId = userId, Email = email });
    }
}
