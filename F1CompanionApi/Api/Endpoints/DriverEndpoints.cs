using System;
using F1CompanionApi.Domain.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace F1CompanionApi.Api.Endpoints;

public static class DriverEndpoints
{
    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/drivers", GetDriversAsync)
            .RequireAuthorization()
            .WithName("GetDrivers")
            .WithOpenApi()
            .WithDescription("Retrieves a list of all drivers");

        app.MapGet("/drivers/{id}", GetDriverByIdAsync)
            .RequireAuthorization()
            .WithName("GetDriverById")
            .WithOpenApi()
            .WithDescription("Retrieves a specific driver by Id");

        return app;
    }

    private static async Task<IResult> GetDriversAsync(
        IDriverService driverService,
        [FromServices] ILogger logger)
    {
        logger.LogDebug("Fetching all drivers");

        var drivers = await driverService.GetDriversAsync();

        return Results.Ok(drivers);
    }

    private static async Task<IResult> GetDriverByIdAsync(
        IDriverService driverService,
        int id,
        [FromServices] ILogger logger)
    {
        logger.LogDebug("Fetching driver {DriverId}", id);

        var driver = await driverService.GetDriverByIdAsync(id);

        if (driver is null)
        {
            logger.LogWarning("Driver {DriverId} not found", id);

            return Results.Problem(
                detail: "Driver not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Results.Ok(driver);
    }
}
