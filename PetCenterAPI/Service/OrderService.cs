using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using PetCenterAPI.Hubs;
using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IHubContext<AppHub> _hub;

        public OrderService(
            IOrderRepository orderRepository,
            IInventoryRepository inventoryRepository,
            IMapper mapper,
            ILogger<OrderService> logger,
            IHubContext<AppHub> hub)
        {
            _orderRepository = orderRepository;
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _logger = logger;
            _hub = hub;
        }

        #region Get & Query Orders

        /// <summary>
        /// Lấy danh sách toàn bộ đơn hàng dành cho Admin, có hỗ trợ OData query (phân trang, lọc, sắp xếp)
        /// </summary>
        public async Task<List<ReadOrderListDTO>> GetOrderListAdminAsync(ODataQueryOptions<ReadOrderListDTO> queryOptions)
        {
            _logger.LogInformation("Admin đang truy vấn danh sách đơn hàng thông qua OData.");
            try
            {
                var query = _orderRepository.GetAllOrders()
                    .ProjectTo<ReadOrderListDTO>(_mapper.ConfigurationProvider);

                var filtered = (IQueryable<ReadOrderListDTO>)queryOptions.ApplyTo(query);
                return await filtered.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình truy vấn danh sách đơn hàng Admin.");
                throw;
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng cụ thể dựa vào OrderId
        /// </summary>
        public async Task<ReadOrderDetailDTO?> GetOrderDetailsAsync(Guid orderId)
        {
            _logger.LogInformation($"Bắt đầu lấy chi tiết đơn hàng ID: {orderId}");

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning($"Không tìm thấy đơn hàng ID: {orderId}");
                return null;
            }

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
                    ProductName = snapshot?.ProductName ?? "Product Details Unavailable",
                    ProductCategory = snapshot?.ProductCategory ?? "N/A",
                    ProductBrand = snapshot?.ProductBrand ?? "N/A",
                    ProductImage = snapshot?.ProductImage ?? "https://example.com/default.jpg",
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice
                });
            }

            _logger.LogInformation($"Lấy thành công chi tiết đơn hàng ID: {orderId} với {dto.OrderItems.Count} sản phẩm.");
            return dto;
        }

        /// <summary>
        /// Lấy lịch sử mua hàng của một khách hàng cụ thể
        /// </summary>
        public async Task<List<ReadOrderListDTO>> GetCustomerOrderHistoryAsync(Guid customerId)
        {
            _logger.LogInformation($"Truy xuất lịch sử đơn hàng cho Customer ID: {customerId}");
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId);

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

        #endregion

        #region Order Status & Inventory Management

        /// <summary>
        /// Hủy đơn hàng và xử lý hoàn trả số lượng sản phẩm về kho
        /// </summary>
        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            _logger.LogInformation($"Bắt đầu tiến trình hủy đơn hàng ID: {orderId}");

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning($"Hủy thất bại: Không tìm thấy đơn hàng ID: {orderId}");
                throw new KeyNotFoundException("Order not found");
            }

            if (order.Status == 3 || order.Status == 4)
            {
                _logger.LogWarning($"Hủy thất bại: Đơn hàng {orderId} đang ở trạng thái {order.Status} (Đang giao hoặc Đã hoàn thành).");
                throw new InvalidOperationException("Cannot cancel an order that is shipping or completed.");
            }

            // Cập nhật trạng thái đơn hàng
            order.Status = 0; // 0 = Cancelled
            order.UpdateAt = DateTime.UtcNow;

            // Xử lý logic hoàn kho
            _logger.LogInformation($"Tiến hành hoàn trả số lượng cho {order.OrderDetails.Count} loại sản phẩm trong đơn hàng {orderId}");
            foreach (var detail in order.OrderDetails)
            {
                var inventory = await _inventoryRepository.GetInventoryByProductIdAsync(detail.ProductId);
                if (inventory != null)
                {
                    inventory.QuantityReserved -= detail.Quantity;
                    inventory.QuantityAvailable += detail.Quantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                    _logger.LogInformation($"Đã hoàn {detail.Quantity} sản phẩm (Product ID: {detail.ProductId}) về kho Available.");
                }
                else
                {
                    _logger.LogWarning($"Không tìm thấy Inventory record cho Product ID: {detail.ProductId} khi hủy đơn.");
                }
            }

            await _orderRepository.SaveAsync();
            // Notify admins and the specific customer about cancelled order
            try
            {
                await _hub.Clients.Group("Admins").SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                if (order.CustomerId != Guid.Empty)
                    await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
            }
            catch { }

            _logger.LogInformation($"Hủy đơn hàng ID: {orderId} thành công.");
            return true;
        }

        /// <summary>
        /// Tăng tiến trình trạng thái đơn hàng và trừ kho hàng chờ khi bắt đầu giao
        /// </summary>
        public async Task<int> AdvanceOrderStatusAsync(Guid orderId)
        {
            _logger.LogInformation($"Bắt đầu cập nhật tiến trình cho đơn hàng ID: {orderId}");

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning($"Cập nhật thất bại: Không tìm thấy đơn hàng ID: {orderId}");
                throw new KeyNotFoundException("Order not found");
            }

            if (order.Status == 0 || order.Status == 4)
            {
                _logger.LogWarning($"Cập nhật thất bại: Đơn hàng {orderId} đã Hủy (0) hoặc Hoàn thành (4).");
                throw new InvalidOperationException("Cannot advance status of a cancelled or completed order.");
            }

            int oldStatus = order.Status;
            order.Status += 1;
            order.UpdateAt = DateTime.UtcNow;

            // Xử lý kho khi trạng thái chuyển sang Đang Giao Hàng (Status = 3)
            if (order.Status == 3)
            {
                _logger.LogInformation($"Đơn hàng {orderId} chuyển sang trạng thái Đang giao (3). Tiến hành trừ số lượng Reserved trong kho.");
                foreach (var detail in order.OrderDetails)
                {
                    var inventory = await _inventoryRepository.GetInventoryByProductIdAsync(detail.ProductId);
                    if (inventory != null)
                    {
                        inventory.QuantityReserved -= detail.Quantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                        _logger.LogInformation($"Đã giải phóng {detail.Quantity} sản phẩm (Product ID: {detail.ProductId}) khỏi hàng đợi Reserved.");
                    }
                }
            }

            // Ghi nhận thời gian giao hàng thành công
            if (order.Status == 4)
            {
                _logger.LogInformation($"Đơn hàng {orderId} đã giao thành công. Đang cập nhật DeliveredDate.");
                order.DeliveredDate = DateTime.UtcNow;
            }

            await _orderRepository.SaveAsync();

            // Notify admins and the specific customer about updated order status
            try
            {
                await _hub.Clients.Group("Admins").SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                if (order.CustomerId != Guid.Empty)
                    await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
            }
            catch { }

            _logger.LogInformation($"Cập nhật trạng thái đơn hàng {orderId} thành công: {oldStatus} -> {order.Status}");

            return order.Status;
        }

        #endregion
    }
}