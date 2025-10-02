using System;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Domain.Services;

public interface ILeagueService
{
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

    public async Task<IEnumerable<League>> GetLeaguesAsync()
    {
        return await _dbContext.Leagues
            .Include(x => x.Owner)
            .ToListAsync();
    }

    public async Task<League?> GetLeagueByIdAsync(int id)
    {
        return await _dbContext.Leagues.FirstOrDefaultAsync(x => x.Id == id);
    }
}
