namespace PetCenterClient.ViewModels
{
    // DTO Dùng chung để Thêm/Sửa
    public class MutatePetViewModel
    {
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    // OData Wrapper (Rất quan trọng để hứng dữ liệu OData)
    public class ODataResponse<T>
    {
        [System.Text.Json.Serialization.JsonPropertyName("value")]
        public List<T> Value { get; set; } = new();
    }

    // ================= CUSTOMER DTOs =================
    public class ReadPetListViewModel
    {
        public Guid PetId { get; set; }
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string? PetAvatar { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        // Tự động tính tuổi ở Client
        public string Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return "Unknown";
                var age = DateTime.Today.Year - DateOfBirth.Value.Year;
                return age > 0 ? $"{age} years" : "Under 1 year";
            }
        }
    }

    public class ReadPetDetailViewModel : ReadPetListViewModel
    {
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
    }

    // ================= VET/ADMIN DTOs =================
    public class ReadVetPetListViewModel : ReadPetListViewModel
    {
        public string OwnerName { get; set; } = null!;
        public string OwnerPhone { get; set; } = null!;
    }

    public class ReadVetPetDetailViewModel : ReadVetPetListViewModel
    {
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
    }
}