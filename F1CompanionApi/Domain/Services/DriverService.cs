using F1CompanionApi.Api.Mappers;
using F1CompanionApi.Api.Models;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;

namespace F1CompanionApi.Domain.Services;

public interface IDriverService
{
    Task<IEnumerable<DriverResponse>> GetDriversAsync();
    Task<DriverResponse?> GetDriverByIdAsync(int id);
}

public class DriverService : IDriverService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DriverService> _logger;

    public DriverService(ApplicationDbContext dbContext, ILogger<DriverService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<DriverResponse>> GetDriversAsync()
    {
        _logger.LogDebug("Fetching all drivers");

        var drivers = await _dbContext.Drivers.ToListAsync();

        _logger.LogDebug("Retrieved {DriverCount} drivers", drivers.Count);

        return drivers.ToResponseModel();
    }

    public async Task<DriverResponse?> GetDriverByIdAsync(int id)
    {
        _logger.LogDebug("Fetching driver with id {id}", id);

        var driver = await _dbContext.Drivers.FirstOrDefaultAsync(x => x.Id == id);

        if (driver is null)
        {
            _logger.LogWarning("Driver {DriverId} not found", id);
        }

        return driver?.ToResponseModel();
    }
}
