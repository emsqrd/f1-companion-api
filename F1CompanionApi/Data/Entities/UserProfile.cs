using System.Text.Json.Serialization;

namespace F1CompanionApi.Data.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public required string AccountId { get; set; }
    public string? DisplayName { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    //TODO: Remove this when implementing DTOs
    [JsonIgnore]
    public Account Account { get; set; } = null!;
}
