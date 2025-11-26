using System.Text.Json.Serialization;

namespace F1CompanionApi.Data.Entities;

/// <summary>
/// Base class for entities that are created and owned by users.
/// Extends BaseEntity with user audit fields and navigation properties.
/// Use this for user-generated content like Teams, Leagues, etc.
/// </summary>
public abstract class UserOwnedEntity : BaseEntity
{
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public int? DeletedBy { get; set; }

    [JsonIgnore]
    public UserProfile CreatedByUser { get; set; } = null!;

    [JsonIgnore]
    public UserProfile? UpdatedByUser { get; set; }

    [JsonIgnore]
    public UserProfile? DeletedByUser { get; set; }
}
