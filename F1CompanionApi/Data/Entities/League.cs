namespace F1CompanionApi.Data.Models;

public class League
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int OwnerId { get; set; }

    public UserProfile Owner { get; set; } = null!;
}
