namespace PetCenterClient.DTOs
{
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; } 
        public int TotalOrders { get; set; }   
        public int PendingOrders { get; set; } 
        public List<RevenueByMonthDto> RevenueByMonth { get; set; } = new();
    }

    public class RevenueByMonthDto
    {
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }
}