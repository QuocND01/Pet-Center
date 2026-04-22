using System;
using System.Collections.Generic;

namespace AttendanceAPI.Models;

public partial class Attendance
{
    public Guid AttendanceId { get; set; }

    public Guid ShiftId { get; set; }

    public Guid StaffId { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual StaffShift Shift { get; set; } = null!;
}
