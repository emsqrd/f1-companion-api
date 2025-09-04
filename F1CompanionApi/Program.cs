using F1CompanionApi.Endpoints;
using Scalar.AspNetCore;
using F1CompanionApi.Extensions;
using F1CompanionApi.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddOpenApi();

builder.AddApplicationServices();

var app = builder.Build();

app.UseCors("AllowedOrigins");
app.UseMiddleware<SupabaseAuthMiddleware>();
app.UseAuthorization();

app.MapEndpoints()
.MapOpenApi();

app.MapScalarApiReference();

app.Run();
