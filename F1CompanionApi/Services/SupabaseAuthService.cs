using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Supabase.Gotrue;

namespace F1CompanionApi.Services;

public interface ISupabaseAuthService
{
    ClaimsPrincipal? ValidateToken(string token);
    string? GetUserId(ClaimsPrincipal user);
    string? GetUserEmail(ClaimsPrincipal user);
    IEnumerable<string> GetAllClaims(ClaimsPrincipal user);
}

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly string _jwtSecret;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public SupabaseAuthService(IConfiguration configuration)
    {
        _jwtSecret = configuration["Supabase:JwtSecret"] ??
            throw new InvalidOperationException("Supabase JWT secret not configured");
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ??
            user.FindFirst("email")?.Value;
    }

    // Add this temporary method for debugging
    public IEnumerable<string> GetAllClaims(ClaimsPrincipal user)
    {
        return user.Claims.Select(c => $"{c.Type}: {c.Value}");
    }
}
