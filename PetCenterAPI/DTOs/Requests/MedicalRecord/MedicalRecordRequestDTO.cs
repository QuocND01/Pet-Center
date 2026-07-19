using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.MedicalRecord
{
    public static class MedicalRecordRequestDTO
    {
        public class CreateMedicalRecordDTO
        {
            [Required]
            public Guid AppointmentId { get; set; }

            public Guid? DiseaseId { get; set; }

            [MaxLength(255)]
            public string? CustomDiseaseName { get; set; }

            [Required]
            [MaxLength(255)]
            public string Diagnosis { get; set; } = null!;

            [Required]
            [MaxLength(255)]
            public string Treatment { get; set; } = null!;

            [MaxLength(255)]
            public string? Note { get; set; }
        }

        public class UpdateMedicalRecordDTO
        {
            public Guid? DiseaseId { get; set; }

            [MaxLength(255)]
            public string? CustomDiseaseName { get; set; }

            [Required]
            [MaxLength(255)]
            public string Diagnosis { get; set; } = null!;

            [Required]
            [MaxLength(255)]
            public string Treatment { get; set; } = null!;

            [MaxLength(255)]
            public string? Note { get; set; }
        }
    }
}
