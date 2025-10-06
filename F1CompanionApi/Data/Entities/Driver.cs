namespace F1CompanionApi.Data.Entities;

public class Driver : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? CountryAbbreviation { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int? Number { get; set; }
    public decimal Price { get; set; }
    public int Points { get; set; }
}