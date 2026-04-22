using System;
using System.Collections.Generic;

namespace AttendanceAPI.Models;

public partial class StaffShift
{
    public Guid ShiftId { get; set; }

    public Guid StaffId { get; set; }

    public DateOnly ShiftDate { get; set; }

    public TimeOnly? OverrideStartTime { get; set; }

    public TimeOnly? OverrideEndTime { get; set; }

    public int? Status { get; set; }

    public Guid TemplateId { get; set; }

    public string? Note { get; set; }

    public virtual Attendance? Attendance { get; set; }

    public virtual ShiftTemplate Template { get; set; } = null!;
}
