namespace PetCenterClient.ViewModels.Appointment
{
    public class AvailableSlotViewModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int GapBeforeMinutes { get; set; }
        public int GapAfterMinutes { get; set; }

        public int Score { get; set; }

        public bool IsRecommended { get; set; }

        public int? RecommendationRank { get; set; }
    }
    public class GetAvailableSlotsRequestViewModel
    {
        public Guid StaffId { get; set; }

        public DateOnly Date { get; set; }

        public List<Guid> ServiceIds { get; set; } = [];
    }
}
