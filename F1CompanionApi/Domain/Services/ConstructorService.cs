using F1CompanionApi.Api.Mappers;
using F1CompanionApi.Api.Models;
using F1CompanionApi.Data;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Domain.Services;

public interface IConstructorService
{
    Task<IEnumerable<ConstructorResponse>> GetConstructorsAsync();
    Task<ConstructorResponse?> GetConstructorByIdAsync(int id);
}

public class ConstructorService : IConstructorService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ConstructorService> _logger;

    public ConstructorService(ApplicationDbContext dbContext, ILogger<ConstructorService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ConstructorResponse>> GetConstructorsAsync()
    {
        var constructors = await _dbContext.Constructors.ToListAsync();

        _logger.LogDebug("Retrieved {ConstructorCount} constructors", constructors.Count);

        return constructors.ToResponseModel();
    }

    public async Task<ConstructorResponse?> GetConstructorByIdAsync(int id)
    {
        var constructor = await _dbContext.Constructors.FirstOrDefaultAsync(x => x.Id == id);

        if (constructor is null)
        {
            _logger.LogWarning("Constructor {ConstructorId} not found", id);
        }

        return constructor?.ToResponseModel();
    }

}
