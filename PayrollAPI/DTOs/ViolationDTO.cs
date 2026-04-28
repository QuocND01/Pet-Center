using System;

namespace PayrollAPI.DTOs
{
    public class ViolationResponseDTO
    {
        public Guid ViolationId { get; set; }
        public Guid StaffId { get; set; }
        public string ViolationType { get; set; } = null!;
        public decimal Penalty { get; set; }
        public DateTime ViolationDate { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ViolationRequestDTO
    {
        public Guid StaffId { get; set; }
        public string ViolationType { get; set; } = null!;
        public decimal Penalty { get; set; }
        public DateTime ViolationDate { get; set; }
    }

    // Gộp Search, Filter, View List
    public class ViolationQueryParameters
    {
        public Guid? StaffId { get; set; } // Tìm lịch sử của 1 nhân viên
        public string? SearchTerm { get; set; } // Tìm kiếm theo loại vi phạm (ViolationType)
        public int? Status { get; set; } // Lọc theo trạng thái
        public DateTime? FromDate { get; set; } // Lọc theo ngày vi phạm
        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}