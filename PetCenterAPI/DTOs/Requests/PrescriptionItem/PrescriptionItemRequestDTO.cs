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
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
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
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }

            [MaxLength(255)]
            public string? Note { get; set; }
        }
    }
}
