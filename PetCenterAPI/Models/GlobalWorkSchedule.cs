namespace PetCenterAPI.Models
{
    public class GlobalWorkSchedule
    {
        public Guid GlobalScheduleId { get; set; }

        /// <summary>
        /// 1 = Monday ... 7 = Sunday
        /// </summary>
        public byte DayOfWeek { get; set; }

        public bool IsWorking { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

    }
}
