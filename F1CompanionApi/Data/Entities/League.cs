namespace F1CompanionApi.Data.Entities;

public class League : BaseEntity
{
    public required string Name { get; set; }
    public int OwnerId { get; set; }

    public UserProfile Owner { get; set; } = null!;
}
