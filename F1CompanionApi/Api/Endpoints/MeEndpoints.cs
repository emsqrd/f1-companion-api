using F1CompanionApi.Data.Models;
using F1CompanionApi.Domain.Services;

namespace F1CompanionApi.Api.Endpoints;

public static class MeEndpoints
{
    public record RegisterUserRequest(string? DisplayName);

    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me/profile", GetUserProfileAsync)
            .WithName("Get User Profile")
            .WithOpenApi()
            .WithDescription("Gets user profile")
            .RequireAuthorization();

        app.MapPost("/me/register", RegisterUserAsync)
            .WithName("Register User")
            .WithOpenApi()
            .WithDescription("Creates user account and profile")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetUserProfileAsync(HttpContext context, ISupabaseAuthService authService, IUserProfileService userProfileService)
    {
        var userId = authService.GetUserId(context.User);

        //TODO: replace null forgiving operators with proper null checks
        var user = await userProfileService.GetUserProfileByAccountIdAsync(userId!);

        if (user is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(user);
    }

    private static async Task<IResult> RegisterUserAsync(HttpContext httpContext, ISupabaseAuthService authService, IUserProfileService userProfileService, RegisterUserRequest request)
    {
        var userId = authService.GetUserId(httpContext.User);
        var userEmail = authService.GetUserEmail(httpContext.User);

        var existingUser = await userProfileService.GetUserProfileByAccountIdAsync(userId!);
        if (existingUser != null)
        {
            return Results.Conflict("User already registered");
        }

        var userProfile = await userProfileService.CreateUserProfileAsync(userId!, userEmail!, request.DisplayName);

        return Results.Created($"/me/profile", userProfile);
    }

    // private static async Task<IResult> UpdateUserProfileAsync(HttpContext httpContext, ISupabaseAuthService, IUserProfileService userProfileService, UserProfile userProfile)
    // {

    // }
}
