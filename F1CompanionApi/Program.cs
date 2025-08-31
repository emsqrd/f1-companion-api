using F1CompanionApi.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapEndpoints()
.MapOpenApi();

app.MapScalarApiReference();

app.Run();
