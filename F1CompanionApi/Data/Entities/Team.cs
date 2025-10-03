namespace F1CompanionApi.Data.Models;

public class Team
{
    public int Id { get; set; }
    public int Rank { get; set; }
    public required string Name { get; set; }
    public required string OwnerName { get; set; }
    public int TotalPoints { get; set; }

    //TODO: Remove Rank and TotalPoints from here and calculate them on the fly
    // Navigation properties
    // public List<Round> Rounds { get; set; } = new();

    // Calculated property
    // public int TotalPoints => Rounds.Sum(r => r.Points);
}