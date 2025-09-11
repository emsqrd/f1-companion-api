namespace F1CompanionApi.Api.Models;

public class UpdateUserProfileRequest
{
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
}
