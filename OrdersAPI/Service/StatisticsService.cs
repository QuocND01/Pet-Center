using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using PetCenterAPI.DTOs;

namespace PetCenterAPI.Service
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