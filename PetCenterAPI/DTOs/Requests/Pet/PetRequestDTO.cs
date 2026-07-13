using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using PetCenterAPI.DTOs.Requests.Pet;

namespace PetCenterAPI.DTOs.Requests.CustomerProfile
{
    public class PetRequestDTO
    {
        // DTO hiển thị danh sách (gọn nhẹ)
        public class ReadPetListDTO
        {
            public Guid PetId { get; set; }
            public string PetName { get; set; } = null!;
            public string Species { get; set; } = null!;
            public string Breed { get; set; } = null!;
            public string Gender { get; set; } = null!;
            public string? PetAvatar { get; set; }
            public DateOnly? DateOfBirth { get; set; }
            public bool IsActive { get; set; }
        }

        // DTO hiển thị chi tiết (đầy đủ)
        public class ReadPetDetailDTO : ReadPetListDTO
        {
            public decimal? Weight { get; set; }
            public string? Note { get; set; }
            public DateOnly? DateOfBirth { get; set; }
        }

        public class MutatePetDTO
        {
            [Required(ErrorMessage = "Pet name is required")]
            [RegularExpression(@"^[\p{L}\p{N}\s'\-]+$", ErrorMessage = "Pet name contains invalid characters")]
            public string PetName { get; set; } = null!;

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
    }
}