using F1CompanionApi.Endpoints;
using Scalar.AspNetCore;
using F1CompanionApi.Extensions;
using F1CompanionApi.Data;
using Microsoft.EntityFrameworkCore;

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

app.MapEndpoints()
.MapOpenApi();

app.MapScalarApiReference();

app.Run();
