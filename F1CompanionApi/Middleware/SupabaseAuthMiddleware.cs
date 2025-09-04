using System;
using F1CompanionApi.Services;

namespace F1CompanionApi.Middleware;

public class SupabaseAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SupabaseAuthService _authService;

    public SupabaseAuthMiddleware(RequestDelegate next, SupabaseAuthService authService)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    private string? ExtractTokenFromHeader(HttpRequest request)
    {
        var authHeader = request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer") == true)
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromHeader(context.Request);

        if (!string.IsNullOrEmpty(token))
        {
            var principal = _authService.ValidateToken(token);
            if (principal != null)
            {
                context.User = principal;
            }
        }

        await _next(context);
    }
}
