namespace CustomerAPI.DTOs.Pet;

public class PetReadDto
{
    public Guid PetId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public string? Gender { get; set; }
    public decimal? Weight { get; set; }
    public string? Note { get; set; }
    public bool? IsActive { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}