using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Disease
{
    public class DiseaseDTO
    {
        public class ReadDiseaseDTO
        {
            public Guid DiseaseId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public string? Recommendation { get; set; }
            public int Species { get; set; } // 1: Dog, 2: Cat, 3: Both... (Tùy logic frontend của bạn)
            public bool IsSystem { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class MutateDiseaseDTO
        {
            [Required(ErrorMessage = "Name is required")]
            [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
            public string Name { get; set; } = null!;

            [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string? Description { get; set; }

            [StringLength(2000, ErrorMessage = "Recommendation cannot exceed 2000 characters")]
            public string? Recommendation { get; set; }

            public int Species { get; set; }
        }
    }
}