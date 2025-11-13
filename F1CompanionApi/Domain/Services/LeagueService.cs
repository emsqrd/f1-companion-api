using F1CompanionApi.Api.Models;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using F1CompanionApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace F1CompanionApi.Domain.Services;

public interface ILeagueService
{
    Task<LeagueResponseModel> CreateLeagueAsync(
        CreateLeagueRequest createLeagueRequest,
        int ownerId
    );
    Task<IEnumerable<League>> GetLeaguesAsync();
    Task<League?> GetLeagueByIdAsync(int id);
    Task<IEnumerable<League>> GetLeaguesByOwnerIdAsync(int ownerId);
}

public class LeagueService : ILeagueService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LeagueService> _logger;

    public LeagueService(ApplicationDbContext dbContext, ILogger<LeagueService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LeagueResponseModel> CreateLeagueAsync(
        CreateLeagueRequest createLeagueRequest,
        int ownerId
    )
    {
        _logger.LogDebug("Creating league {LeagueName} for owner {OwnerId}",
            createLeagueRequest.Name, ownerId);

        var owner = await _dbContext.UserProfiles.FindAsync(ownerId);
        if (owner is null)
        {
            _logger.LogError("Owner {OwnerId} not found when creating league", ownerId);
            throw new InvalidOperationException($"Owner with id {ownerId} not found");
        }

        var newLeague = new League
        {
            Name = createLeagueRequest.Name,
            Description = createLeagueRequest.Description,
            OwnerId = ownerId,
            CreatedBy = ownerId,
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Leagues.AddAsync(newLeague);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully created league {LeagueId} with name {LeagueName} for owner {OwnerId}",
            newLeague.Id, newLeague.Name, ownerId);

        return new LeagueResponseModel
        {
            Id = newLeague.Id,
            Name = newLeague.Name,
            Description = newLeague.Description,
            OwnerName = owner.GetFullName(),
            MaxTeams = newLeague.MaxTeams,
            IsPrivate = newLeague.IsPrivate,
        };
    }

    // TODO: Update these endpoints to return LeagueResponseModel instead of League
    public async Task<IEnumerable<League>> GetLeaguesAsync()
    {
        _logger.LogDebug("Fetching all leagues");
        var leagues = await _dbContext.Leagues.Include(x => x.Owner).ToListAsync();
        _logger.LogDebug("Retrieved {LeagueCount} leagues", leagues.Count);
        return leagues;
    }

    public async Task<League?> GetLeagueByIdAsync(int id)
    {
        _logger.LogDebug("Fetching league {LeagueId}", id);
        var league = await _dbContext.Leagues.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == id);

        if (league is null)
        {
            _logger.LogWarning("League {LeagueId} not found", id);
        }

        return league;
    }

    public async Task<IEnumerable<League>> GetLeaguesByOwnerIdAsync(int ownerId)
    {
        _logger.LogDebug("Fetching leagues for owner {OwnerId}", ownerId);
        var leagues = await _dbContext.Leagues
            .Include(x => x.Owner)
            .Where(x => x.OwnerId == ownerId)
            .ToListAsync();
        _logger.LogDebug("Retrieved {LeagueCount} leagues for owner {OwnerId}", leagues.Count, ownerId);
        return leagues;
    }
}
