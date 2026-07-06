namespace PetCenterClient.DTOs
{
    public class ReadVetPetListViewModel
    {
        public Guid PetId { get; set; }
        public string Species { get; set; } = null!;
        public string Breed { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string Age { get; set; } = null!;
        public string? PetAvatar { get; set; }
        public string OwnerName { get; set; } = null!;
        public string OwnerPhone { get; set; } = null!;
    }

    public class ReadVetPetDetailViewModel : ReadVetPetListViewModel
    {
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}