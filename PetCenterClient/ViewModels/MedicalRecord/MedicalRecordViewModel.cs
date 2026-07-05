using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.MedicalRecord
{
    public static class MedicalRecordViewModel
    {
        public class ReadMedicalRecordListViewModel
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

        public class ReadMedicalRecordDetailViewModel
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
            public DateTime AppointmentEnd { get; set; }
            public string CustomerName { get; set; } = null!;
            public string PetSpecies { get; set; } = null!;
            public string PetBreed { get; set; } = null!;
            public string VetName { get; set; } = null!;
            public List<PrescriptionItem.PrescriptionItemViewModel.ReadPrescriptionItemViewModel> PrescriptionItems { get; set; } = new();
        }

        public class CreateMedicalRecordViewModel
        {
            [Required]
            public Guid AppointmentId { get; set; }

            [Display(Name = "Disease")]
            public Guid? DiseaseId { get; set; }

            [MaxLength(200)]
            [Display(Name = "Custom Disease Name")]
            public string? CustomDiseaseName { get; set; }

            [Required(ErrorMessage = "Diagnosis is required")]
            [MaxLength(500)]
            [Display(Name = "Diagnosis")]
            public string Diagnosis { get; set; } = null!;

            [Required(ErrorMessage = "Treatment is required")]
            [MaxLength(500)]
            [Display(Name = "Treatment")]
            public string Treatment { get; set; } = null!;

            [MaxLength(500)]
            [Display(Name = "Note")]
            public string? Note { get; set; }
        }

        public class UpdateMedicalRecordViewModel
        {
            public Guid RecordId { get; set; }

            [Display(Name = "Disease")]
            public Guid? DiseaseId { get; set; }

            [MaxLength(200)]
            [Display(Name = "Custom Disease Name")]
            public string? CustomDiseaseName { get; set; }

            [Required(ErrorMessage = "Diagnosis is required")]
            [MaxLength(500)]
            [Display(Name = "Diagnosis")]
            public string Diagnosis { get; set; } = null!;

            [Required(ErrorMessage = "Treatment is required")]
            [MaxLength(500)]
            [Display(Name = "Treatment")]
            public string Treatment { get; set; } = null!;

            [MaxLength(500)]
            [Display(Name = "Note")]
            public string? Note { get; set; }
        }

        public class ReadDiseaseViewModel
        {
            public Guid DiseaseId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public string? Recommendation { get; set; }
            public int Species { get; set; }
        }

        public class ChangeStatusViewModel
        {
            public Guid RecordId { get; set; }
            public string Diagnosis { get; set; } = null!;
            public int CurrentStatus { get; set; }
            public string CurrentStatusName { get; set; } = null!;
            public int TargetStatus { get; set; }
            public string TargetStatusName { get; set; } = null!;
        }

        public class CompletedAppointmentViewModel
        {
            public Guid AppointmentId { get; set; }
            public DateTime AppointmentStart { get; set; }
            public string CustomerName { get; set; } = null!;
            public string PetSpecies { get; set; } = null!;
            public string PetBreed { get; set; } = null!;
            public string VetName { get; set; } = null!;
            public string DisplayText { get; set; } = null!;
        }
    }
}
