using System.ComponentModel.DataAnnotations;

namespace CustomerAPI.DTOs.Pet;

public class PetCreateDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required, MaxLength(100)]
    public string Species { get; set; } = null!;

    [MaxLength(100)]
    public string? Breed { get; set; }

    [MaxLength(100)]
    public string? Gender { get; set; }

    [Range(0.01, 999.99)]
    public decimal? Weight { get; set; }

    [MaxLength(255)]
    public string? Note { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}