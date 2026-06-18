using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<List<ReadOrderListDTO>> GetOrderListAdminAsync(ODataQueryOptions<ReadOrderListDTO> queryOptions)
        {
            var query = _orderRepository.GetAllOrders()
                .ProjectTo<ReadOrderListDTO>(_mapper.ConfigurationProvider);

            // ApplyTo sẽ tự động dịch các param trên URL (như $filter, $orderby, $skip, $top) thành câu SQL
            var filtered = (IQueryable<ReadOrderListDTO>)queryOptions.ApplyTo(query);

            return await filtered.ToListAsync();
        }

        public async Task<ReadOrderDetailDTO?> GetOrderDetailsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null) return null;

            var dto = new ReadOrderDetailDTO
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                DeliveredDate = order.DeliveredDate,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                AddressSnapshot = order.AddressSnapshot,
                CustomerName = order.Customer?.FullName ?? "Unknown",
                PhoneNumber = order.Customer?.PhoneNumber ?? "N/A",
                Email = order.Customer?.Email ?? "N/A"
            };

            foreach (var detail in order.OrderDetails)
            {
                var snapshot = detail.OrderProductSnapshot;
                dto.OrderItems.Add(new ReadOrderItemDTO
                {
                    ProductId = detail.ProductId,
                    ProductName = snapshot?.ProductName ?? "Product Details Unvailable",
                    ProductCategory = snapshot?.ProductCategory ?? "N/A",
                    ProductBrand = snapshot?.ProductBrand ?? "N/A",
                    ProductImage = snapshot?.ProductImage ?? "https://example.com/default.jpg",
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice
                });
            }

            return dto;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            // Ràng buộc nghiệp vụ: Đang giao (3) hoặc Đã hoàn thành (4) thì không cho hủy
            if (order.Status == 3 || order.Status == 4)
            {
                throw new InvalidOperationException("Cannot cancel an order that is shipping or completed.");
            }

            order.Status = 0; // Set to Cancelled
            order.UpdateAt = DateTime.UtcNow;

            await _orderRepository.SaveAsync();
            return true;
        }

        public async Task<int> AdvanceOrderStatusAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            // Nếu đơn hàng đã hủy (0) hoặc đã hoàn thành (4) thì không tiến tới được nữa
            if (order.Status == 0 || order.Status == 4)
            {
                throw new InvalidOperationException("Cannot advance status of a cancelled or completed order.");
            }

            order.Status += 1; // Tăng tiến trình lên nấc tiếp theo (1 -> 2 -> 3 -> 4)
            order.UpdateAt = DateTime.UtcNow;

            // Nếu trạng thái tiến tới Complete (4), có thể cập nhật thêm DeliveredDate
            if (order.Status == 4)
            {
                order.DeliveredDate = DateTime.UtcNow;
            }

            await _orderRepository.SaveAsync();
            return order.Status;
        }
        public async Task<List<ReadOrderListDTO>> GetCustomerOrderHistoryAsync(Guid customerId)
        {
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId);

            // Tự map thủ công hoặc dùng AutoMapper (_mapper.Map<...>)
            var dtoList = orders.Select(o => new ReadOrderListDTO
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer?.FullName ?? "Unknown",
                PhoneNumber = o.Customer?.PhoneNumber ?? "N/A",
                OrderDate = o.OrderDate ?? DateTime.UtcNow,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                AddressSnapshot = o.AddressSnapshot
            }).ToList();

            return dtoList;
        }
    }
}