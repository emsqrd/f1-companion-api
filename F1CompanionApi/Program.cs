using F1CompanionApi.Api.Endpoints;
using F1CompanionApi.Data;
using F1CompanionApi.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddOpenApi();

builder.AddApplicationServices();

var app = builder.Build();

app.UseCors("AllowedOrigins");
app.UseAuthentication();
app.UseAuthorization();

// Apply latest migrations to the database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.MapEndpoints().MapOpenApi();

app.MapScalarApiReference();

app.Run();
