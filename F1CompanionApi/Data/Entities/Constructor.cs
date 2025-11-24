namespace F1CompanionApi.Data.Entities;

public class Constructor : BaseEntity
{
    public required string Name { get; set; }
    public string? FullName { get; set; }
    public required string CountryAbbreviation { get; set; }
    public bool IsActive { get; set; }
}
