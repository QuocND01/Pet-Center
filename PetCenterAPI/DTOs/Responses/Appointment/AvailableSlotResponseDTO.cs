namespace PetCenterAPI.DTOs.Responses.Appointment
{
    public class AvailableSlotResponseDTO
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int GapBeforeMinutes { get; set; }

        public int GapAfterMinutes { get; set; }

        public int Score { get; set; }

        public bool IsRecommended { get; set; }
        public int? RecommendationRank { get; set; }
    }
}
