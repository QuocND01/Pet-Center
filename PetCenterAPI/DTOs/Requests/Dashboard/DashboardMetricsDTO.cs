namespace PetCenterAPI.Models
{
    public class DashboardMetricsDTO
    {
        // Các chỉ số KPI tổng quan
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; } // DÒNG BỊ THIẾU NẰM Ở ĐÂY NÈ
        public int TotalOrders { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalCustomers { get; set; }

        // Mảng dữ liệu cho biểu đồ 6 tháng (Nếu bạn vẫn giữ logic cũ)
        public List<MonthlyRevenueDTO> RevenueChart { get; set; } = new();

        // Các mảng dữ liệu cho phiên bản Pro (ChartItemDTO đa năng)
        public List<ChartItemDTO> RevenueTimeline { get; set; } = new();
        public List<ChartItemDTO> CategoryChart { get; set; } = new();
        public List<ChartItemDTO> TopProducts { get; set; } = new();
        public List<ChartItemDTO> TopServices { get; set; } = new();
    }

    public class MonthlyRevenueDTO
    {
        public string Month { get; set; } = null!;
        public decimal Revenue { get; set; }
    }

    public class TopProductDTO
    {
        public string ProductName { get; set; } = null!;
        public int TotalSold { get; set; }
    }

    // Class đa năng dùng cho mọi biểu đồ của phiên bản Pro
    public class ChartItemDTO
    {
        public string Label { get; set; } = null!;
        public decimal Value { get; set; }
    }
}