namespace F1CompanionApi.Data.Entities;

/// <summary>
/// Represents a fantasy F1 team with selected drivers and constructors.
/// </summary>
public class Team : UserOwnedEntity
{
    public required string Name { get; set; }
    public int UserId { get; set; } // FK

    public UserProfile Owner { get; set; } = null!;
}