using System.Text.Json.Serialization;

namespace F1CompanionApi.Data.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    [JsonIgnore]
    public UserProfile CreatedByUser { get; set; } = null!;

    [JsonIgnore]
    public UserProfile? UpdatedByUser { get; set; }

    [JsonIgnore]
    public UserProfile? DeletedByUser { get; set; }
}
