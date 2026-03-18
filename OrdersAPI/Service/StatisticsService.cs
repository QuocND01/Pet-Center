using OrdersAPI.Repository;
using OrdersAPI.Repository.Interface;
using OrdersAPI.Service.Interface;
using OrdersAPI.DTOs;

namespace OrdersAPI.Service
{
    public class StatisticsService : IStatisticsService 
{
    private readonly IOrderRepository _orderRepo;
    public StatisticsService(IOrderRepository orderRepo) => _orderRepo = orderRepo;

    public async Task<DashboardStatsDto> GetAdminDashboardStatsAsync(int year) 
    {
        return new DashboardStatsDto {
            TotalRevenue = await _orderRepo.GetTotalRevenueAsync(year),
            TotalOrders = await _orderRepo.GetTotalOrdersAsync(year),
            PendingOrders = await _orderRepo.GetPendingOrdersAsync(year),
            RevenueByMonth = await _orderRepo.GetRevenueByMonthAsync(year)
        };
    }
}
}