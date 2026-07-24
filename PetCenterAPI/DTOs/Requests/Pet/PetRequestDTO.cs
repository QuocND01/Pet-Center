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
            [StringLength(100, ErrorMessage = "PetName cannot exceed 100 characters")]
            public string PetName { get; set; } = null!;

            [Required(ErrorMessage = "Species is required")]
            [StringLength(100, ErrorMessage = "Species cannot exceed 100 characters")]
            public string Species { get; set; } = null!;

            [Required(ErrorMessage = "Breed is required")]
            [StringLength(100, ErrorMessage = "Breed cannot exceed 100 characters")]
            public string Breed { get; set; } = null!;

            [Required(ErrorMessage = "Gender is required")]
            [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters")]
            public string Gender { get; set; } = null!;

            [Range(0, 10000, ErrorMessage = "Weight must be between 0 and 10000")]
            public decimal? Weight { get; set; }

            [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters")]
            public string? Note { get; set; }

            public DateOnly? DateOfBirth { get; set; }

            public IFormFile? ImageFile { get; set; }
        }
    }
}