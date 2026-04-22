using System;
using System.Collections.Generic;

namespace AttendanceAPI.Models;

public partial class ShiftTemplate
{
    public Guid TemplateId { get; set; }

    public Guid RoleId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int DaysOfWeek { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
}
