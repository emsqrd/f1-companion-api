using System;
using F1CompanionApi.Api.Models;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Domain.Services;

public interface ILeagueService
{
    Task<LeagueResponseModel> CreateLeagueAsync(
        CreateLeagueRequest createLeagueRequest,
        int ownerId
    );
    Task<IEnumerable<League>> GetLeaguesAsync();
    Task<League?> GetLeagueByIdAsync(int id);
}

public class LeagueService : ILeagueService
{
    private readonly ApplicationDbContext _dbContext;

    public LeagueService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<LeagueResponseModel> CreateLeagueAsync(
        CreateLeagueRequest createLeagueRequest,
        int ownerId
    )
    {
        var owner = await _dbContext.UserProfiles.FindAsync(ownerId);
        if (owner is null)
        {
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

        return new LeagueResponseModel
        {
            Id = newLeague.Id,
            Name = newLeague.Name,
            Description = newLeague.Description,
            OwnerName = owner.FullName,
            MaxTeams = newLeague.MaxTeams,
            IsPrivate = newLeague.IsPrivate,
        };
    }

    public async Task<IEnumerable<League>> GetLeaguesAsync()
    {
        return await _dbContext.Leagues.Include(x => x.Owner).ToListAsync();
    }

    public async Task<League?> GetLeagueByIdAsync(int id)
    {
        return await _dbContext.Leagues.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == id);
    }
}
