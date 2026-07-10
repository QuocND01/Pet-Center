using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.Models
{
    public class Disease
    {
        public Guid DiseaseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(2000)]
        public string Description { get; set; } = null!;

        [StringLength(2000)]
        public string Recommendation { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int Species { get; set; }

        public bool IsActive { get; set; }
        public bool IsSystem { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ICollection<MedicalRecord> MedicalRecords { get; set; }
            = new List<MedicalRecord>();
    }
}
