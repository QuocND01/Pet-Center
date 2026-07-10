namespace PetCenterAPI.DTOs.Requests
{
    public class VetPetRequestDTO
    {
        public class ReadVetPetListDTO
        {
            public Guid PetId { get; set; }
            public string PetName { get; set; } = null!;
            public string Species { get; set; } = null!;
            public string Breed { get; set; } = null!;
            public string Gender { get; set; } = null!;
            public DateOnly? DateOfBirth { get; set; }
            public string? PetAvatar { get; set; }

            // Thêm thông tin Chủ nhân cho Vet xem
            public string OwnerName { get; set; } = null!;
            public string OwnerPhone { get; set; } = null!;
        }

        public class ReadVetPetDetailDTO : ReadVetPetListDTO
        {
            public decimal? Weight { get; set; }
            public string? Note { get; set; }
            public DateOnly? DateOfBirth { get; set; }
        }
    }
}