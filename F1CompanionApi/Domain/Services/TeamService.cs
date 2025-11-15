using F1CompanionApi.Api.Models;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using F1CompanionApi.Extensions;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Domain.Services;

public interface ITeamService
{
    Task<TeamResponseModel> CreateTeamAsync(CreateTeamRequest request, int userId);
    Task<TeamResponseModel?> GetUserTeamAsync(int userId);
}

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TeamService> _logger;

    public TeamService(
        ApplicationDbContext dbContext,
        ILogger<TeamService> logger
    )
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TeamResponseModel> CreateTeamAsync(CreateTeamRequest request, int userId)
    {
        _logger.LogInformation("Creating team for user {UserId}", userId);

        // Check if user already has a team
        var existingTeam = await _dbContext.Teams.FirstOrDefaultAsync(t => t.UserId == userId);

        if (existingTeam is not null)
        {
            _logger.LogWarning("User {UserId} already has a team {TeamId}", userId, existingTeam.Id);
            throw new InvalidOperationException("User already has a team");
        }

        // Get user profile from owner name
        var user = await _dbContext.UserProfiles.FindAsync(userId);
        if (user is null)
        {
            _logger.LogError("User {UserId} not found", userId);
            throw new InvalidOperationException("User not found");
        }

        var team = new Team
        {
            Name = request.Name.Trim(),
            UserId = userId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Team {TeamId} created for user {UserId}", team.Id, userId);
        return new TeamResponseModel
        {
            Id = team.Id,
            Name = team.Name,
            OwnerName = user.GetFullName(),
        };
    }

    public async Task<TeamResponseModel?> GetUserTeamAsync(int userId)
    {
        _logger.LogDebug("Fetching team for user {UserId}", userId);

        var team = await _dbContext.Teams
            .Include(t => t.Owner)
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (team is null)
        {
            _logger.LogWarning("Team not found for User {UserId}", userId);
            return null;
        }

        return new TeamResponseModel
        {
            Id = team.Id,
            Name = team.Name,
            OwnerName = team.Owner.GetFullName(),
        };
    }
}
