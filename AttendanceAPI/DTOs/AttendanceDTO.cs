using System;

namespace AttendanceAPI.DTOs
{
    public class AttendanceResponseDTO
    {
        public Guid AttendanceId { get; set; }
        public Guid ShiftId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AttendanceRequestDTO
    {
        public Guid ShiftId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? Status { get; set; }
    }

    // Gộp View List, Search và Filter
    public class AttendanceQueryParameters
    {
        public Guid? StaffId { get; set; } // Tìm kiếm lịch sử của riêng 1 nhân viên
        public Guid? ShiftId { get; set; } // Tìm theo ca cụ thể
        public int? Status { get; set; } // Lọc theo trạng thái (Đi muộn, vắng mặt...)
        public DateTime? FromDate { get; set; } // Lọc theo ngày check-in
        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}