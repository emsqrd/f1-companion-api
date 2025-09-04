using F1CompanionApi.Data;
using F1CompanionApi.Services;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Extensions;

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
                    policy.WithOrigins(allowedOrigins)
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
        services.AddSingleton<SupabaseAuthService>();
        services.AddAuthorization();
    }
}
