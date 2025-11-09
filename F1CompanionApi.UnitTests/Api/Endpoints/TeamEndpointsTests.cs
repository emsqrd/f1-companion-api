using F1CompanionApi.Api.Endpoints;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.UnitTests.Api.Endpoints;

public class TeamEndpointsTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetTeams_MultipleTeams_ReturnsAllTeams()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var teams = new List<Team>
        {
            new Team
            {
                Name = "Team Alpha",
                OwnerName = "John Doe",
                Rank = 1,
                TotalPoints = 100
            },
            new Team
            {
                Name = "Team Beta",
                OwnerName = "Jane Smith",
                Rank = 2,
                TotalPoints = 85
            },
            new Team
            {
                Name = "Team Gamma",
                OwnerName = "Bob Johnson",
                Rank = 3,
                TotalPoints = 70
            }
        };

        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();

        // Act
        var result = await InvokeGetTeams(context);

        // Assert
        Assert.NotNull(result);
        var teamList = result.ToList();
        Assert.Equal(3, teamList.Count);
        Assert.Contains(teamList, t => t.Name == "Team Alpha");
        Assert.Contains(teamList, t => t.Name == "Team Beta");
        Assert.Contains(teamList, t => t.Name == "Team Gamma");
    }

    [Fact]
    public async Task GetTeamByIdAsync_ExistingTeam_ReturnsTeam()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var team = new Team
        {
            Name = "Findable Team",
            OwnerName = "Team Owner",
            Rank = 1,
            TotalPoints = 200
        };

        context.Teams.Add(team);
        await context.SaveChangesAsync();

        // Act
        var result = await InvokeGetTeamByIdAsync(team.Id, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(team.Id, result.Id);
        Assert.Equal("Findable Team", result.Name);
        Assert.Equal("Team Owner", result.OwnerName);
        Assert.Equal(1, result.Rank);
        Assert.Equal(200, result.TotalPoints);
    }

    [Fact]
    public async Task GetTeamByIdAsync_NonExistentTeam_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        var result = await InvokeGetTeamByIdAsync(999, context);

        // Assert
        Assert.Null(result);
    }

    // Helper methods to invoke private endpoint methods via reflection
    private async Task<IEnumerable<Team>> InvokeGetTeams(ApplicationDbContext db)
    {
        var method = typeof(TeamEndpoints).GetMethod(
            "GetTeams",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IEnumerable<Team>>)method!.Invoke(
            null,
            new object[] { db }
        )!;

        return await task;
    }

    private async Task<Team?> InvokeGetTeamByIdAsync(int id, ApplicationDbContext db)
    {
        var method = typeof(TeamEndpoints).GetMethod(
            "GetTeamByIdAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<Team>)method!.Invoke(
            null,
            new object[] { id, db }
        )!;

        return await task;
    }
}
