using Microsoft.AspNetCore.Http;

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
            public string PetName { get; set; } = null!;
            public string Species { get; set; } = null!;
            public string Breed { get; set; } = null!;
            public string Gender { get; set; } = null!;
            public decimal? Weight { get; set; }
            public string? Note { get; set; }
            public DateOnly? DateOfBirth { get; set; }
            public IFormFile? ImageFile { get; set; }
        }
    }
}