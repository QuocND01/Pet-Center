using System.Numerics;

namespace PetCenterAPI.Models
{
    public class ScheduleException
    {
        public Guid ExceptionId { get; set; }

        public Guid? StaffId { get; set; }

        public DateOnly ExceptionDate { get; set; }

        public bool IsWorking { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public string? Reason { get; set; }

        public virtual Staff? Staff { get; set; }
    }
}
