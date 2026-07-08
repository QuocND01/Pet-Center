namespace PetCenterClient.ViewModels
{
    public class ReadDiseaseViewModel
    {
        public Guid DiseaseId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Recommendation { get; set; }
        public int Species { get; set; }
        public bool IsSystem { get; set; }
        public DateTime CreatedAt { get; set; }

        // Helper để hiển thị tên loài cho đẹp trên UI
        public string SpeciesName => Species == 1 ? "Dog 🐶" : (Species == 2 ? "Cat 🐱" : "Dog & Cat 🐾");
    }

    public class MutateDiseaseViewModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Recommendation { get; set; }
        public int Species { get; set; }
    }
}