namespace PetCenterClient.ViewModels
{
    public class DashboardMetricsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalCustomers { get; set; }
        public List<MonthlyRevenueViewModel> RevenueChart { get; set; } = new();
        public List<TopProductViewModel> TopProducts { get; set; } = new();
    }

    public class MonthlyRevenueViewModel
    {
        public string Month { get; set; } = null!;
        public decimal Revenue { get; set; }
    }

    public class TopProductViewModel
    {
        public string ProductName { get; set; } = null!;
        public int TotalSold { get; set; }
    }
}