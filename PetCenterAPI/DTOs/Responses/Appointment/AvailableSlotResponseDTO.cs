namespace PetCenterAPI.DTOs.Responses.Appointment
{
    public class AvailableSlotResponseDTO
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsAvailable { get; set; }
    }
}
