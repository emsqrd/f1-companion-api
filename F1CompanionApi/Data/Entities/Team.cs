namespace F1CompanionApi.Data.Entities;

public class Team : BaseEntity
{
    public int Rank { get; set; }
    public required string Name { get; set; }
    public required string OwnerName { get; set; }
    public int TotalPoints { get; set; }
    public int UserId { get; set; } // FK

    public UserProfile Owner { get; set; } = null!;
}