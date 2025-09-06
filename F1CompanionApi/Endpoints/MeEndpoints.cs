using System;
using F1CompanionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace F1CompanionApi.Endpoints;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me/profile", GetUserProfileAsync)
            .WithName("Get User Profile")
            .WithOpenApi()
            .WithDescription("Gets user profile")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetUserProfileAsync(HttpContext context, ISupabaseAuthService authService, IUserProfileService userProfileService)
    {
        var userId = authService.GetUserId(context.User);

        var user = await userProfileService.GetUserProfileByAccountIdAsync(userId!);

        if (user is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(user);
    }
}
