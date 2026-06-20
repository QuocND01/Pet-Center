using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.MedicalRecord
{
    public static class MedicalRecordRequestDTO
    {
        public class CreateMedicalRecordDTO
        {
            [Required]
            public Guid AppointmentId { get; set; }

            [Required]
            [MaxLength(500)]
            public string Diagnosis { get; set; } = null!;

            [Required]
            [MaxLength(500)]
            public string Treatment { get; set; } = null!;

            [MaxLength(500)]
            public string? Note { get; set; }
        }

        public class UpdateMedicalRecordDTO
        {
            [Required]
            [MaxLength(500)]
            public string Diagnosis { get; set; } = null!;

            [Required]
            [MaxLength(500)]
            public string Treatment { get; set; } = null!;

            [MaxLength(500)]
            public string? Note { get; set; }
        }
    }
}
