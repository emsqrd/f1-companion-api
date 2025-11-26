namespace F1CompanionApi.Data.Entities;

public class Driver : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Abbreviation { get; set; }
    public required string CountryAbbreviation { get; set; }
    public bool IsActive { get; set; }
}