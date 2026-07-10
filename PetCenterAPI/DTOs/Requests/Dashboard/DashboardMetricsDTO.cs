namespace PetCenterAPI.Models
{
    public class DashboardMetricsDTO
    {
        // Các chỉ số tổng quan
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalCustomers { get; set; }

        // Dữ liệu cho Biểu đồ doanh thu 6 tháng gần nhất
        public List<MonthlyRevenueDTO> RevenueChart { get; set; } = new();

        // Top 5 sản phẩm bán chạy nhất
        public List<TopProductDTO> TopProducts { get; set; } = new();
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
}