public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; } // Tổng tiền thu về
    public int TotalOrders { get; set; }    // Tổng số đơn hàng
    public int PendingOrders { get; set; }  // Đơn hàng đang chờ xử lý
    public List<MonthlyRevenueDto> RevenueByMonth { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}