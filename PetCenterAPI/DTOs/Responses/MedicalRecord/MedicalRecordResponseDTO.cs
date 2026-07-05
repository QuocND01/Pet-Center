using PetCenterAPI.DTOs.Responses.PrescriptionItem;

namespace PetCenterAPI.DTOs.Responses.MedicalRecord
{
    public static class MedicalRecordResponseDTO
    {
        public class ReadMedicalRecordDTO
        {
            public Guid RecordId { get; set; }
            public Guid? AppointmentId { get; set; }
            public Guid CustomerId { get; set; }
            public Guid? DiseaseId { get; set; }
            public string? DiseaseNameSnapshot { get; set; }
            public string Diagnosis { get; set; } = null!;
            public string Treatment { get; set; } = null!;
            public string? Note { get; set; }
            public DateTime? CreatedAt { get; set; }
            public int? Status { get; set; }
            public string StatusName { get; set; } = null!;

            // Appointment info
            public DateTime AppointmentStart { get; set; }
            public DateTime AppointmentEnd { get; set; }

            // Customer info
            public string CustomerName { get; set; } = null!;

            // Pet info (from snapshot)
            public string PetSpecies { get; set; } = null!;
            public string PetBreed { get; set; } = null!;

            // Vet info (from snapshot)
            public string VetName { get; set; } = null!;

            // Prescription items
            public List<PrescriptionItemResponseDTO.ReadPrescriptionItemDTO> PrescriptionItems { get; set; } = new();
        }

        public class ReadMedicalRecordListDTO
        {
            public Guid RecordId { get; set; }
            public Guid? AppointmentId { get; set; }
            public Guid CustomerId { get; set; }
            public Guid? DiseaseId { get; set; }
            public string? DiseaseNameSnapshot { get; set; }
            public string Diagnosis { get; set; } = null!;
            public string Treatment { get; set; } = null!;
            public string? Note { get; set; }
            public DateTime? CreatedAt { get; set; }
            public int? Status { get; set; }
            public string StatusName { get; set; } = null!;
            public DateTime AppointmentStart { get; set; }
            public string CustomerName { get; set; } = null!;
            public string PetSpecies { get; set; } = null!;
            public string PetBreed { get; set; } = null!;
            public string VetName { get; set; } = null!;
        }

        public class CompletedAppointmentDTO
        {
            public Guid AppointmentId { get; set; }
            public DateTime AppointmentStart { get; set; }
            public string CustomerName { get; set; } = null!;
            public string PetSpecies { get; set; } = null!;
            public string PetBreed { get; set; } = null!;
            public string VetName { get; set; } = null!;
            public string DisplayText { get; set; } = null!;
        }

        public class ReadDiseaseDTO
        {
            public Guid DiseaseId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public string? Recommendation { get; set; }
            public int Species { get; set; }
        }
    }
}
