using System;

namespace AttendanceAPI.DTOs
{
    public class ShiftTemplateResponseDTO
    {
        public Guid TemplateId { get; set; }
        public Guid RoleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DaysOfWeek { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ShiftTemplateRequestDTO
    {
        public Guid RoleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DaysOfWeek { get; set; }
    }

    // Gộp Search và View List vào đây
    public class ShiftTemplateQueryParameters
    {
        public Guid? RoleId { get; set; } // Lọc/Tìm kiếm theo Role
        public bool? IsActive { get; set; } // Lọc theo trạng thái đang mở/đã đóng
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}