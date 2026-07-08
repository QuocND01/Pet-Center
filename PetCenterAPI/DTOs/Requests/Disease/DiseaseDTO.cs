namespace PetCenterAPI.DTOs.Requests
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
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public string? Recommendation { get; set; }
            public int Species { get; set; }
        }
    }
}