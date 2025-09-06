namespace F1CompanionApi.Data.Models;

public class Account
{
    public required string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public UserProfile? Profile { get; set; }
}
