using System.Text.Json.Serialization;

namespace F1CompanionApi.Data.Entities;

public class Account
{
    public required string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime DeletedAt { get; set; }
    public int DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    [JsonIgnore]
    public UserProfile? Profile { get; set; }
}
