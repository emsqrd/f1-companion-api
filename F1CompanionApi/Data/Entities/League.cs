namespace F1CompanionApi.Data.Entities;

public class League : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int MaxTeams { get; set; } = 15;
    public bool IsPrivate { get; set; } = true;
    public required int OwnerId { get; set; }

    public UserProfile Owner { get; set; } = null!;
}
