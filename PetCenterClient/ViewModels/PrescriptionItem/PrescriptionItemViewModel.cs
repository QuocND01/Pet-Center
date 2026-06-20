using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.PrescriptionItem
{
    public static class PrescriptionItemViewModel
    {
        public class ReadPrescriptionItemViewModel
        {
            public Guid PrescriptionItemId { get; set; }
            public Guid RecordId { get; set; }
            public string MedicineName { get; set; } = null!;
            public string Dosage { get; set; } = null!;
            public string Duration { get; set; } = null!;
            public int Quantity { get; set; }
            public string? Note { get; set; }
            public int Status { get; set; }
            public string StatusName { get; set; } = null!;
        }

        public class CreatePrescriptionItemViewModel
        {
            [Required]
            public Guid RecordId { get; set; }

            [Required(ErrorMessage = "Medicine name is required")]
            [MaxLength(255)]
            [Display(Name = "Medicine Name")]
            public string MedicineName { get; set; } = null!;

            [Required(ErrorMessage = "Dosage is required")]
            [MaxLength(255)]
            [Display(Name = "Dosage")]
            public string Dosage { get; set; } = null!;

            [Required(ErrorMessage = "Duration is required")]
            [MaxLength(255)]
            [Display(Name = "Duration")]
            public string Duration { get; set; } = null!;

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            [Display(Name = "Quantity")]
            public int Quantity { get; set; }

            [MaxLength(255)]
            [Display(Name = "Note")]
            public string? Note { get; set; }
        }

        public class UpdatePrescriptionItemViewModel
        {
            public Guid PrescriptionItemId { get; set; }

            [Required(ErrorMessage = "Medicine name is required")]
            [MaxLength(255)]
            [Display(Name = "Medicine Name")]
            public string MedicineName { get; set; } = null!;

            [Required(ErrorMessage = "Dosage is required")]
            [MaxLength(255)]
            [Display(Name = "Dosage")]
            public string Dosage { get; set; } = null!;

            [Required(ErrorMessage = "Duration is required")]
            [MaxLength(255)]
            [Display(Name = "Duration")]
            public string Duration { get; set; } = null!;

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            [Display(Name = "Quantity")]
            public int Quantity { get; set; }

            [MaxLength(255)]
            [Display(Name = "Note")]
            public string? Note { get; set; }
        }

        public class DeletePrescriptionItemViewModel
        {
            public Guid PrescriptionItemId { get; set; }
            public Guid RecordId { get; set; }
            public string MedicineName { get; set; } = null!;
            public int Status { get; set; }
        }
    }
}
