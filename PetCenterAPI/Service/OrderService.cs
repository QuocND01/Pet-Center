using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCenterAPI.Common;
using PetCenterAPI.Hubs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

namespace PetCenterAPI.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryTransactionRepository _invenTransactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IHubContext<AppHub> _hub;

        public OrderService(
            IOrderRepository orderRepository,
            IInventoryRepository inventoryRepository,
            IInventoryTransactionRepository inventoryTransactionReposotory,
            IMapper mapper,
            ILogger<OrderService> logger,
            IHubContext<AppHub> hub)
        {
            _orderRepository = orderRepository;
            _inventoryRepository = inventoryRepository;
            _invenTransactionRepository = inventoryTransactionReposotory;
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
            _logger.LogInformation($"[AdvanceOrder] Bắt đầu cập nhật tiến trình cho đơn hàng ID: {orderId}");

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning($"[AdvanceOrder] Cập nhật thất bại: Không tìm thấy đơn hàng ID: {orderId}");
                throw new KeyNotFoundException("Order not found");
            }

            _logger.LogInformation($"[AdvanceOrder] Đơn hàng ID: {orderId} hiện có Status = {order.Status}");

            if (order.Status == 0 || order.Status == 4)
            {
                _logger.LogWarning($"[AdvanceOrder] Cập nhật thất bại: Đơn hàng {orderId} đang ở trạng thái Hủy (0) hoặc Hoàn thành (4). Không thể tăng tiếp.");
                throw new InvalidOperationException("Cannot advance status of a cancelled or completed order.");
            }

            int oldStatus = order.Status;
            order.Status += 1;
            order.UpdateAt = DateTime.UtcNow;

            // Ghi nhận thời gian giao hàng thành công NẾU trạng thái mới là Hoàn thành (4)
            if (order.Status == 4)
            {
                _logger.LogInformation($"[AdvanceOrder] Đơn hàng {orderId} đã chuyển sang Hoàn thành (4). Cập nhật DeliveredDate.");
                order.DeliveredDate = DateTime.UtcNow;
            }

            // Chỉ xử lý kho khi chuyển sang Shipping (từ 2 sang 3)
            if (oldStatus == 2 && order.Status == 3)
            {
                _logger.LogInformation($"[FIFO/FEFO] Phát hiện chuyển trạng thái 2 -> 3. Bắt đầu xử lý kho cho Đơn hàng: {orderId}");

                foreach (var detail in order.OrderDetails)
                {
                    _logger.LogInformation($"[FIFO/FEFO] Xử lý Sản phẩm ID: {detail.ProductId} | Số lượng yêu cầu: {detail.Quantity}");

                    // 1. Kiểm tra tổng Inventory
                    var inventory = await _inventoryRepository.GetInventoryByProductIdAsync(detail.ProductId);
                    if (inventory == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy tồn kho của sản phẩm {detail.ProductId}");
                    }

                    if (inventory.QuantityReserved < detail.Quantity)
                    {
                        throw new InvalidOperationException($"Reserved không đủ cho sản phẩm {detail.ProductId}");
                    }

                    // 2. Lấy danh sách lô hàng FEFO/FIFO
                    var batches = await _inventoryRepository.GetAvailableBatchesByProductIdAsync(detail.ProductId);
                    if (!batches.Any())
                    {
                        throw new InvalidOperationException($"Không có lô hàng khả dụng cho sản phẩm {detail.ProductId}");
                    }

                    var totalBatchStock = batches.Sum(x => x.StockLeft);
                    if (totalBatchStock < detail.Quantity)
                    {
                        throw new InvalidOperationException($"Không đủ tồn kho batch cho sản phẩm {detail.ProductId}");
                    }

                    // 3. Trừ kho theo từng lô & Ghi nhận Transaction
                    var remainingQty = detail.Quantity;

                    foreach (var batch in batches)
                    {
                        if (remainingQty <= 0) break;

                        var pickedQty = Math.Min(batch.StockLeft, remainingQty);

                        // --- [TÍNH TOÁN CHO TRANSACTION] ---
                        // Giả sử QuantityBefore/After đại diện cho trạng thái của bảng Inventory tổng:
                        int qtyTotal = inventory.QuantityReserved + inventory.QuantityAvailable;
                        int qtyBefore = qtyTotal;
                        int qtyAfter = qtyTotal - pickedQty;

                        // Nếu DB thiết kế QuantityBefore/After đại diện cho Tồn của riêng từng LÔ (Batch), hãy đổi thành:
                        //int qtyBefore = batch.StockLeft;
                        //int qtyAfter = batch.StockLeft - pickedQty;

                        // --- [CẬP NHẬT TRẠNG THÁI BATCH] ---
                        batch.StockLeft -= pickedQty;
                        batch.QuantitySold += pickedQty;

                        if (batch.StockLeft == 0)
                        {
                            batch.BatchStatus = BatchStatus.Exhausted;
                        }

                        // --- [CẬP NHẬT TỔNG KHO CUỐN CHIẾU] ---
                        inventory.QuantityReserved -= pickedQty;

                        remainingQty -= pickedQty;

                        // --- [LƯU LỊCH SỬ TRANSACTION] ---
                        Guid testId = order.CustomerId;
                        await _invenTransactionRepository.AddTransactionAsync(new InventoryTransaction
                        {
                            TransactionId = Guid.NewGuid(),
                            InventoryId = inventory.InventoryId,
                            TransactionType = TransactionType.StockOut, // 'StockOut' khớp CHECK constraint

                            QuantityChange = -pickedQty, // Số âm (Ví dụ: -5)
                            QuantityBefore = qtyBefore,  // Ví dụ: 20
                            QuantityAfter = qtyAfter,    // Ví dụ: 15 (Thỏa mãn: 15 = 20 + (-5))

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = testId,
                            ReferenceId = order.OrderId,
                            ReferenceType = ReferenceType.Order,     // Khớp CHECK constraint ('Order')
                            ImportStockDetailId = batch.ImportStockDetailsId,
                            Note = $"Export form batch {batch.BatchCode}"
                        });
                    }

                    // 4. Cập nhật thời gian update cuối cùng của Inventory tổng
                    inventory.LastUpdated = DateTime.UtcNow;

                    _logger.LogInformation($"Đã giải phóng và trừ lô thành công cho sản phẩm {detail.ProductId}");
                    await _invenTransactionRepository.SaveChange();
                }
            } // <-- Đóng ngoặc chuẩn khối IF xử lý kho

            _logger.LogInformation($"[AdvanceOrder] Lưu thay đổi trạng thái đơn hàng vào Database qua OrderRepository...");
            await _orderRepository.SaveAsync();

            // Bắn SignalR thông báo trạng thái mới
            try
            {
                _logger.LogInformation($"[SignalR] Đang gửi thông báo OrderUpdated lên Hub cho trạng thái mới: {order.Status}");
                await _hub.Clients.Group("Admins").SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                if (order.CustomerId != Guid.Empty)
                {
                    await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[SignalR] Không thể gửi thông báo Realtime (Lỗi bỏ qua được): {ex.Message}");
            }

            _logger.LogInformation($"[AdvanceOrder] Hoàn thành! Trạng thái đơn hàng {orderId} đã đổi: {oldStatus} -> {order.Status}");

            return order.Status;
        }
        #endregion
    }
}