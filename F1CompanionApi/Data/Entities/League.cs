namespace F1CompanionApi.Data.Entities;

public class League : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool MaxTeams { get; set; }
    public bool IsPrivate { get; set; }
    public int OwnerId { get; set; }

    public UserProfile Owner { get; set; } = null!;
}
