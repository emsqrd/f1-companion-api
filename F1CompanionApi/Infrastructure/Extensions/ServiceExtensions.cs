using System.Text;
using F1CompanionApi.Data;
using F1CompanionApi.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace F1CompanionApi.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddServices(builder.Configuration);
        builder.Services.AddDbContext(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            var allowedOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? [];

            options.AddPolicy("AllowedOrigins",
                policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                        {
                            // Check exact matches first
                            if (allowedOrigins.Contains(origin))
                                return true;

                            // Check for Netlify preview deployments
                            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            {
                                return uri.Host.EndsWith(".netlify.app", StringComparison.OrdinalIgnoreCase);
                            }

                            return false;
                        })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
    }

    private static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }

    private static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISupabaseAuthService, SupabaseAuthService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {

            var jwtSecret = configuration["Supabase:JwtSecret"] ??
                throw new InvalidOperationException("Supabase JWT secret not configured");

            var key = Encoding.UTF8.GetBytes(jwtSecret);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
        services.AddScoped<ILeagueService, LeagueService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
    }
}
