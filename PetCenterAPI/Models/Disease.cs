using Microsoft.CodeAnalysis;

namespace PetCenterAPI.Models
{
    public class Disease
    {
        public Guid DiseaseId { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Recommendation { get; set; } = null!;

        public int Species { get; set; }

        public bool IsActive { get; set; }
        public bool IsSystem { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ICollection<MedicalRecord> MedicalRecords { get; set; }
            = new List<MedicalRecord>();
    }
}
