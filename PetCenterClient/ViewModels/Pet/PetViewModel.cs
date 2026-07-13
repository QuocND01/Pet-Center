namespace PetCenterClient.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    // DTO Dùng chung để Thêm/Sửa
    public class MutatePetViewModel
    {
        [Required(ErrorMessage = "Pet name is required")]
        [RegularExpression(@"^[\p{L}\p{N}\s'\-]+$", ErrorMessage = "Pet name contains invalid characters")]
        public string PetName { get; set; } = null!;

        public Guid? CustomerId { get; set; }

        [Required(ErrorMessage = "Species is required")]
        public string Species { get; set; } = null!;

        [Required(ErrorMessage = "Breed is required")]
        public string Breed { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = null!;

        [Range(0, double.MaxValue, ErrorMessage = "Weight must be non-negative")]
        public decimal? Weight { get; set; }

        public string? Note { get; set; }

        [DataType(DataType.Date)]
        [NotLessThanToday(ErrorMessage = "Date of birth cannot be earlier than today")]
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
        public string PetName { get; set; } = null!;
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