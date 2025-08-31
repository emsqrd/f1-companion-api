namespace F1CompanionApi.Data.Models;

public class Team
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string OwnerName { get; set; }
    public int TotalPoints { get; set; }
}