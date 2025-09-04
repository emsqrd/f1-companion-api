using F1CompanionApi.Endpoints;
using F1CompanionApi.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using F1CompanionApi.Extensions;
using F1CompanionApi.Middleware;
using F1CompanionApi.Services;
using Microsoft.AspNetCore.Authorization;

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
