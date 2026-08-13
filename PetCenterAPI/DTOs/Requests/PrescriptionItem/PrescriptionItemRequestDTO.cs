using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.PrescriptionItem
{
    public static class PrescriptionItemRequestDTO
    {
        public class CreatePrescriptionItemDTO
        {
            [Required]
            public Guid RecordId { get; set; }

            [Required]
            [MaxLength(255)]
            public string MedicineName { get; set; } = null!;

            [Required]
            [MaxLength(255)]
            public string Dosage { get; set; } = null!;

            [Required]
            [MaxLength(255)]
            public string Duration { get; set; } = null!;

            [Required]
            [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
            public int Quantity { get; set; }

            [MaxLength(255)]
            public string? Note { get; set; }
        }

        public class UpdatePrescriptionItemDTO
        {
            [Required]
            [MaxLength(255)]
            public string MedicineName { get; set; } = null!;

            [Required]
            [MaxLength(255)]
            public string Dosage { get; set; } = null!;

            [Required]
            [MaxLength(255)]
            public string Duration { get; set; } = null!;

            [Required]
            [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
            public int Quantity { get; set; }

            [MaxLength(255)]
            public string? Note { get; set; }
        }
    }
}
