namespace F1CompanionApi.Data.Entities;

public class League
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int OwnerId { get; set; }

    public UserProfile Owner { get; set; } = null!;
}
