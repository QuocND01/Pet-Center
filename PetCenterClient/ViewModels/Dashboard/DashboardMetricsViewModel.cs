namespace PetCenterClient.ViewModels
{
    public class DashboardMetricsViewModel
    {
        // Các chỉ số KPI tổng quan
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; } // Đã thêm Lợi nhuận
        public int TotalOrders { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalCustomers { get; set; }

        // Các mảng dữ liệu cho phiên bản Analytics Pro
        public List<ChartItemViewModel> RevenueTimeline { get; set; } = new();
        public List<ChartItemViewModel> CategoryChart { get; set; } = new();
        public List<ChartItemViewModel> TopProducts { get; set; } = new();
        public List<ChartItemViewModel> TopServices { get; set; } = new();
    }

    // Class đa năng dùng để vẽ MỌI loại biểu đồ
    public class ChartItemViewModel
    {
        public string Label { get; set; } = null!; // Tên nhãn (Tên SP, Tên ngày, Tên danh mục...)
        public decimal Value { get; set; }         // Giá trị (Doanh thu, Số lượng...)
    }
}