namespace F1CompanionApi.Data.Entities;

public class Team : BaseEntity
{
    public required string Name { get; set; }
    public int UserId { get; set; } // FK

    public UserProfile Owner { get; set; } = null!;
}