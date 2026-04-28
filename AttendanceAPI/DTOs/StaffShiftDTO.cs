using System;

namespace AttendanceAPI.DTOs
{
    public class StaffShiftResponseDTO
    {
        public Guid ShiftId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime ShiftDate { get; set; }
        // StartTime/EndTime sẽ tự động lấy từ Override, nếu không có sẽ lấy từ Template
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? Status { get; set; }
        public Guid TemplateId { get; set; }
        public string? Note { get; set; }
    }

    public class StaffShiftRequestDTO
    {
        public Guid StaffId { get; set; }
        public DateTime ShiftDate { get; set; }
        public Guid TemplateId { get; set; }
        public TimeSpan? OverrideStartTime { get; set; }
        public TimeSpan? OverrideEndTime { get; set; }
        public string? Note { get; set; }
    }

    // Gộp Filter và Search vào đây
    public class StaffShiftQueryParameters
    {
        public Guid? StaffId { get; set; } // Lọc theo nhân viên
        public Guid? TemplateId { get; set; } // Lọc theo loại ca mẫu
        public DateTime? FromDate { get; set; } // Lọc theo ngày
        public DateTime? ToDate { get; set; }
        public int? Status { get; set; } // Lọc theo trạng thái

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}