using Microsoft.AspNetCore.SignalR;
using PetCenterAPI.DTOs.Requests.Order;
using PetCenterAPI.DTOs.Responses.Order;
using PetCenterAPI.Hubs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IVoucherRepository _voucherRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAddressRepository _addressRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IHubContext<AppHub> _hub;
        private readonly IVnPayService _vnPayService;
        private readonly IMoMoService _moMoService;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            IOrderRepository orderRepo,
            IPaymentRepository paymentRepo,
            IVoucherRepository voucherRepo,
            IInventoryRepository inventoryRepo,
            IProductRepository productRepo,
            IAddressRepository addressRepo,
            ICartRepository cartRepo,
            IAppointmentRepository appointmentRepo,
            IHubContext<AppHub> hub,
            IVnPayService vnPayService,
            IMoMoService moMoService,
            ILogger<CheckoutService> logger)
        {
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
            _voucherRepo = voucherRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _addressRepo = addressRepo;
            _cartRepo = cartRepo;
            _appointmentRepo = appointmentRepo;
            _hub = hub;
            _vnPayService = vnPayService;
            _moMoService = moMoService;
            _logger = logger;
        }

        public async Task<PlaceOrderResponseDTO> PlaceCodOrderAsync(PlaceCodOrderDTO dto)
        {
            await using var tx = await _orderRepo.BeginTransactionAsync();
            try
            {
                // ── 0. Validate order constraints ───────────────────────────
                if (dto.Items.Count > 10)
                    throw new InvalidOperationException("Each order can contain a maximum of 10 items.");

                var existingOrders = await _orderRepo.GetOrdersByCustomerIdAsync(dto.CustomerId);
                var activeOrderCount = existingOrders.Count(o => o.Status != 0 && o.Status != 4);
                if (activeOrderCount >= 5)
                    throw new InvalidOperationException("A customer can place a maximum of 5 active orders.");

                // ── 1. Validate address ──────────────────────────────────────
                var address = await _addressRepo.GetAddressByIdAsync(dto.AddressId, dto.CustomerId);

                if (address == null)
                    throw new InvalidOperationException("The address is invalid or does not belong to this account.");

                var addrParts = new[] { address.AddressDetails, address.Ward, address.District, address.Province };
                var addressSnapshot = string.Join(", ", addrParts.Where(s => !string.IsNullOrEmpty(s)));

                // ── 2. Compute subtotal ──────────────────────────────────────
                var subtotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity);

                // ── 3. Voucher discount ──────────────────────────────────────
                decimal discountAmount = 0;
                Voucher? voucher = null;
                if (dto.VoucherId.HasValue)
                {
                    var alreadyUsed = await _voucherRepo.HasCustomerUsedVoucherAsync(dto.CustomerId, dto.VoucherId.Value);
                    if (alreadyUsed)
                        throw new InvalidOperationException("You have already used this voucher.");

                    voucher = await _voucherRepo.GetByIdAsync(dto.VoucherId.Value);

                    if (voucher == null || voucher.IsActive != true)
                        throw new InvalidOperationException("The voucher is invalid or has been deactivated.");

                    if (voucher.ExpiredDate.HasValue && voucher.ExpiredDate.Value < DateTime.Now)
                        throw new InvalidOperationException("The voucher has expired.");

                    if (voucher.MinOrderAmount.HasValue && subtotal < voucher.MinOrderAmount.Value)
                        throw new InvalidOperationException($"A minimum order of {voucher.MinOrderAmount.Value:N0} ₫ is required to apply this voucher.");

                    if (voucher.UseageLimit.HasValue && voucher.UseageLimit.Value <= 0)
                        throw new InvalidOperationException("The voucher has reached its usage limit.");

                    discountAmount = subtotal * (voucher.DiscountPercent ?? 0) / 100m;
                    if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
                        discountAmount = voucher.MaxDiscountAmount.Value;
                }

                var finalAmount = subtotal - discountAmount;

                if (finalAmount > 5000000m)
                    throw new InvalidOperationException("COD orders cannot exceed 5,000,000 ₫.");

                // ── 4. Check inventory ───────────────────────────────────────
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var inventories = await _inventoryRepo.GetInventoriesByProductIdsAsync(productIds);

                foreach (var item in dto.Items)
                {
                    var inv = inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                    if (inv == null)
                        throw new InvalidOperationException("Inventory information for the product was not found.");

                    var available = inv.QuantityAvailable - inv.QuantityReserved;
                    if (available < item.Quantity)
                        throw new InvalidOperationException(
                            $"Product '{inv.Product.ProductName}' does not have enough stock. " +
                            $"(Available: {available}, Requested: {item.Quantity})");
                }

                // ── 5. Create Order ──────────────────────────────────────────
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = dto.CustomerId,
                    AddressId = dto.AddressId,
                    AddressSnapshot = addressSnapshot,
                    OrderDate = DateTime.Now,
                    TotalAmount = finalAmount,
                    DiscountAmount = discountAmount,
                    Status = 1,
                    PaymentMethod = "COD",
                    PaymentStatus = 1,
                    UpdateAt = DateTime.Now
                };
                await _orderRepo.AddOrderAsync(order);

                // ── 6. OrderDetails + Snapshots ──────────────────────────────
                var products = await _productRepo.GetProductsForSnapshotAsync(productIds);

                var orderDetailsList = new List<OrderDetail>();
                var snapshotsList = new List<OrderProductSnapshot>();

                foreach (var item in dto.Items)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);
                    var detail = new OrderDetail
                    {
                        OrderDetailsId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    orderDetailsList.Add(detail);

                    snapshotsList.Add(new OrderProductSnapshot
                    {
                        ProductSnapshotId = Guid.NewGuid(),
                        OrderDetailsId = detail.OrderDetailsId,
                        ProductName = product.ProductName,
                        ProductDescription = product.ProductDescription ?? string.Empty,
                        ProductCategory = product.Category?.CategoryName ?? string.Empty,
                        ProductBrand = product.Brand?.BrandName ?? string.Empty,
                        ProductImage = product.ProductImages.FirstOrDefault()?.ImageUrl ?? string.Empty,
                        ProductPrice = item.UnitPrice
                    });
                }

                await _orderRepo.AddOrderDetailsAsync(orderDetailsList);
                await _orderRepo.AddOrderProductSnapshotsAsync(snapshotsList);

                // ── 7. Reserve inventory ─────────────────────────────────────
                foreach (var item in dto.Items)
                {
                    var inv = inventories.First(i => i.ProductId == item.ProductId);
                    inv.QuantityReserved += item.Quantity;
                    inv.QuantityAvailable -= item.Quantity;
                    inv.LastUpdated = DateTime.Now;
                }

                // ── 8. Mark voucher used + decrement usage limit ─────────────
                if (dto.VoucherId.HasValue && voucher != null)
                {
                    var customerVoucher = await _voucherRepo.GetCustomerVoucherAsync(dto.CustomerId, dto.VoucherId.Value);

                    if (customerVoucher != null)
                    {
                        customerVoucher.IsUsed = true;
                        await _voucherRepo.UpdateCustomerVoucherAsync(customerVoucher);
                    }
                    else
                    {
                        await _voucherRepo.AddCustomerVoucherAsync(new CustomerVoucher
                        {
                            CustomerId = dto.CustomerId,
                            VoucherId = dto.VoucherId.Value,
                            IsUsed = true
                        });
                    }

                    if (voucher.UseageLimit.HasValue)
                    {
                        voucher.UseageLimit = voucher.UseageLimit.Value - 1;
                        await _voucherRepo.UpdateAsync(voucher);
                    }
                }

                // ── 9. Clear cart (only selected items) ──────────────────────
                var cart = await _cartRepo.GetCartWithDetailsAsync(dto.CustomerId);
                if (cart != null)
                {
                    var cartDetailIds = dto.Items.Select(i => i.CartDetailId).Where(id => id != Guid.Empty).ToList();

                    var toRemove = cart.CartDetails
                        .Where(cd => cartDetailIds.Contains(cd.CartDetailsId) || productIds.Contains(cd.ProductId))
                        .ToList();

                    if (toRemove.Any())
                    {
                        _cartRepo.RemoveDetails(toRemove);
                    }
                }

                await _orderRepo.SaveChangesAsync();
                await tx.CommitAsync();

                // Notify admins and the customer about the new order
                try
                {
                    await _hub.Clients.Group("Admins").SendAsync("OrderCreated", new
                    {
                        OrderId = order.OrderId,
                        CustomerId = order.CustomerId,
                        FinalAmount = finalAmount
                    });

                    await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderCreated", new
                    {
                        OrderId = order.OrderId,
                        CustomerId = order.CustomerId,
                        FinalAmount = finalAmount
                    });
                }
                catch { /* swallow hub errors to avoid failing the request */ }

                return new PlaceOrderResponseDTO
                {
                    Success = true,
                    Message = "Order placed successfully!",
                    OrderId = order.OrderId,
                    TotalAmount = subtotal,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount
                };
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync();
                return new PlaceOrderResponseDTO { Success = false, Message = ex.Message };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ONLINE PAYMENT — Create Order + Payment record (no inventory
        //  reservation), then build gateway redirect URL.
        // ═══════════════════════════════════════════════════════════════════
        public async Task<PlaceOnlineOrderResponseDTO> PlaceOnlineOrderAsync(PlaceOnlineOrderDTO dto)
        {
            await using var tx = await _orderRepo.BeginTransactionAsync();
            try
            {
                // ── 0. Validate order constraints ───────────────────────────
                if (dto.Items.Count > 10)
                    throw new InvalidOperationException("Each order can contain a maximum of 10 items.");

                var existingOrders = await _orderRepo.GetOrdersByCustomerIdAsync(dto.CustomerId);
                var activeOrderCount = existingOrders.Count(o => o.Status != 0 && o.Status != 4);
                if (activeOrderCount >= 5)
                    throw new InvalidOperationException("A customer can place a maximum of 5 active orders.");

                // ── 1. Validate address ──────────────────────────────────────
                var address = await _addressRepo.GetAddressByIdAsync(dto.AddressId, dto.CustomerId);

                if (address == null)
                    throw new InvalidOperationException("The address is invalid or does not belong to this account.");

                var addrParts = new[] { address.AddressDetails, address.Ward, address.District, address.Province };
                var addressSnapshot = string.Join(", ", addrParts.Where(s => !string.IsNullOrEmpty(s)));

                // ── 2. Compute subtotal ──────────────────────────────────────
                var subtotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity);

                // ── 3. Voucher discount ──────────────────────────────────────
                decimal discountAmount = 0;
                Voucher? voucher = null;
                if (dto.VoucherId.HasValue)
                {
                    var alreadyUsed = await _voucherRepo.HasCustomerUsedVoucherAsync(dto.CustomerId, dto.VoucherId.Value);
                    if (alreadyUsed)
                        throw new InvalidOperationException("You have already used this voucher.");

                    voucher = await _voucherRepo.GetByIdAsync(dto.VoucherId.Value);

                    if (voucher == null || voucher.IsActive != true)
                        throw new InvalidOperationException("The voucher is invalid or has been deactivated.");

                    if (voucher.ExpiredDate.HasValue && voucher.ExpiredDate.Value < DateTime.Now)
                        throw new InvalidOperationException("The voucher has expired.");

                    if (voucher.MinOrderAmount.HasValue && subtotal < voucher.MinOrderAmount.Value)
                        throw new InvalidOperationException($"A minimum order of {voucher.MinOrderAmount.Value:N0} ₫ is required to apply this voucher.");

                    if (voucher.UseageLimit.HasValue && voucher.UseageLimit.Value <= 0)
                        throw new InvalidOperationException("The voucher has reached its usage limit.");

                    discountAmount = subtotal * (voucher.DiscountPercent ?? 0) / 100m;
                    if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
                        discountAmount = voucher.MaxDiscountAmount.Value;
                }

                var finalAmount = subtotal - discountAmount;

                if (finalAmount > 10000000m)
                    throw new InvalidOperationException("Online payment orders cannot exceed 10,000,000 ₫.");

                // ── 4. Check inventory (availability only, NO reservation) ───
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var inventories = await _inventoryRepo.GetInventoriesByProductIdsAsync(productIds);

                foreach (var item in dto.Items)
                {
                    var inv = inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                    if (inv == null)
                        throw new InvalidOperationException("Inventory information for the product was not found.");

                    if (inv.QuantityAvailable < item.Quantity)
                        throw new InvalidOperationException(
                            $"Product '{inv.Product.ProductName}' does not have enough stock. " +
                            $"(Available: {inv.QuantityAvailable}, Requested: {item.Quantity})");
                }

                // ── 5. Generate unique transaction reference ─────────────────
                var transactionRef = DateTime.Now.Ticks.ToString();

                // ── 6. Create Order (Pending, no inventory touched) ──────────
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = dto.CustomerId,
                    AddressId = dto.AddressId,
                    AddressSnapshot = addressSnapshot,
                    OrderDate = DateTime.Now,
                    TotalAmount = finalAmount,
                    DiscountAmount = discountAmount,
                    Status = 1,            // Pending
                    PaymentMethod = dto.PaymentMethod.ToUpper(),
                    PaymentStatus = 1,     // Pending
                    UpdateAt = DateTime.Now
                };
                await _orderRepo.AddOrderAsync(order);

                // ── 7. OrderDetails + Snapshots ──────────────────────────────
                var products = await _productRepo.GetProductsForSnapshotAsync(productIds);

                var orderDetailsList = new List<OrderDetail>();
                var snapshotsList = new List<OrderProductSnapshot>();

                foreach (var item in dto.Items)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);
                    var detail = new OrderDetail
                    {
                        OrderDetailsId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    orderDetailsList.Add(detail);

                    snapshotsList.Add(new OrderProductSnapshot
                    {
                        ProductSnapshotId = Guid.NewGuid(),
                        OrderDetailsId = detail.OrderDetailsId,
                        ProductName = product.ProductName,
                        ProductDescription = product.ProductDescription ?? string.Empty,
                        ProductCategory = product.Category?.CategoryName ?? string.Empty,
                        ProductBrand = product.Brand?.BrandName ?? string.Empty,
                        ProductImage = product.ProductImages.FirstOrDefault()?.ImageUrl ?? string.Empty,
                        ProductPrice = item.UnitPrice
                    });
                }

                await _orderRepo.AddOrderDetailsAsync(orderDetailsList);
                await _orderRepo.AddOrderProductSnapshotsAsync(snapshotsList);

                // ── 8. Create Payment record (Pending) ──────────────────────
                var payment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    PaymentMethod = dto.PaymentMethod.ToUpper(),
                    Amount = finalAmount,
                    Status = 1,            // Pending
                    TransactionRef = transactionRef,
                    CreatedAt = DateTime.Now
                };
                await _paymentRepo.AddPaymentAsync(payment);

                // ── 9. Mark voucher used + decrement usage limit ─────────────
                if (dto.VoucherId.HasValue && voucher != null)
                {
                    var customerVoucher = await _voucherRepo.GetCustomerVoucherAsync(dto.CustomerId, dto.VoucherId.Value);

                    if (customerVoucher != null)
                    {
                        customerVoucher.IsUsed = true;
                        await _voucherRepo.UpdateCustomerVoucherAsync(customerVoucher);
                    }
                    else
                    {
                        await _voucherRepo.AddCustomerVoucherAsync(new CustomerVoucher
                        {
                            CustomerId = dto.CustomerId,
                            VoucherId = dto.VoucherId.Value,
                            IsUsed = true
                        });
                    }

                    if (voucher.UseageLimit.HasValue)
                    {
                        voucher.UseageLimit = voucher.UseageLimit.Value - 1;
                        await _voucherRepo.UpdateAsync(voucher);
                    }
                }

                // ── 10. Clear cart (only selected items) ─────────────────────
                var cart = await _cartRepo.GetCartWithDetailsAsync(dto.CustomerId);
                if (cart != null)
                {
                    var cartDetailIds = dto.Items.Select(i => i.CartDetailId).Where(id => id != Guid.Empty).ToList();

                    var toRemove = cart.CartDetails
                        .Where(cd => cartDetailIds.Contains(cd.CartDetailsId) || productIds.Contains(cd.ProductId))
                        .ToList();

                    if (toRemove.Any())
                    {
                        _cartRepo.RemoveDetails(toRemove);
                    }
                }

                await _orderRepo.SaveChangesAsync();
                await tx.CommitAsync();

                // ── 11. Build payment gateway URL ────────────────────────────
                string? paymentUrl = null;
                var orderInfo = $"PetCenter Order {order.OrderId}";

                if (dto.PaymentMethod.Equals("VNPAY", StringComparison.OrdinalIgnoreCase))
                {
                    paymentUrl = _vnPayService.CreatePaymentUrl(
                        order.OrderId, finalAmount, transactionRef,
                        dto.ClientIpAddress, orderInfo);
                }
                else if (dto.PaymentMethod.Equals("MOMO", StringComparison.OrdinalIgnoreCase))
                {
                    paymentUrl = await _moMoService.CreatePaymentUrlAsync(
                        order.OrderId, finalAmount, transactionRef, orderInfo);
                }

                if (string.IsNullOrEmpty(paymentUrl))
                {
                    _logger.LogError("[OnlineCheckout] Failed to generate payment URL for order {OrderId}", order.OrderId);
                    return new PlaceOnlineOrderResponseDTO
                    {
                        Success = false,
                        Message = "Failed to create payment URL. Please try again or choose a different payment method."
                    };
                }

                // Notify admins and the customer about the new order
                try
                {
                    await _hub.Clients.Group("Admins").SendAsync("OrderCreated", new
                    {
                        OrderId = order.OrderId,
                        CustomerId = order.CustomerId,
                        FinalAmount = finalAmount
                    });

                    await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderCreated", new
                    {
                        OrderId = order.OrderId,
                        CustomerId = order.CustomerId,
                        FinalAmount = finalAmount
                    });
                }
                catch { /* swallow hub errors to avoid failing the request */ }

                return new PlaceOnlineOrderResponseDTO
                {
                    Success = true,
                    Message = "Order created. Redirecting to payment gateway...",
                    OrderId = order.OrderId,
                    TotalAmount = subtotal,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    PaymentUrl = paymentUrl,
                    TransactionRef = transactionRef
                };
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync();
                return new PlaceOnlineOrderResponseDTO { Success = false, Message = ex.Message };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  PAYMENT CALLBACK — Idempotent handler for VNPay/MoMo IPN.
        //  On success: deduct inventory (no reservation involved),
        //  create InventoryTransactions, update Payment & Order statuses.
        //  On failure: mark Payment failed, cancel Order.
        // ═══════════════════════════════════════════════════════════════════
        public async Task<PlaceOrderResponseDTO> ProcessPaymentCallbackAsync(
    string transactionRef,
    string gatewayTransactionNo,
    string responseCode,
    string bankCode,
    decimal paidAmount,
    string rawResponse,
    bool isSuccess)
        {
            // ── 1. Find Payment by TransactionRef ────────────────────────
            var payment = await _paymentRepo.GetPaymentByTransactionRefAsync(transactionRef);

            if (payment == null)
            {
                _logger.LogWarning("[PaymentCallback] Payment not found for TransactionRef: {Ref}", transactionRef);
                return new PlaceOrderResponseDTO
                {
                    Success = false,
                    Message = "Payment record not found for the given transaction reference."
                };
            }

            // ── 2. Idempotency check (2: Success, 3: Failed) ─────────────
            if (payment.Status == 2 || payment.Status == 3)
            {
                _logger.LogInformation("[PaymentCallback] Payment {Ref} has already been processed (Status={Status}). Skipping.",
                    transactionRef, payment.Status);
                return new PlaceOrderResponseDTO
                {
                    Success = true,
                    Message = "This payment has already been processed.",
                    OrderId = payment.OrderId != Guid.Empty ? payment.OrderId : payment.AppointmentId
                };
            }

            // ── 3. CẢI TIẾN: Phân nhánh xử lý APPOINTMENT vs ORDER ───────

            // =============================================================
            // NHÁNH A: XỬ LÝ CHO APPOINTMENT (LỊCH HẸN)
            // =============================================================
            if (payment.AppointmentId.HasValue && payment.AppointmentId.Value != Guid.Empty)
            {
                var appointment = await _appointmentRepo.GetByIdAsync(payment.AppointmentId.Value);

                if (appointment == null)
                {
                    return new PlaceOrderResponseDTO
                    {
                        Success = false,
                        Message = "Appointment not found for this payment."
                    };
                }

                // Cập nhật thông tin Payment từ Cổng thanh toán
                payment.GatewayTransactionNo = gatewayTransactionNo;
                payment.ResponseCode = responseCode;
                payment.BankCode = bankCode;
                payment.RawResponse = rawResponse;
                payment.UpdatedAt = DateTime.Now;

                if (isSuccess)
                {
                    payment.Status = 2; // Success
                    payment.PaidAmount = paidAmount;
                    payment.PaidAt = DateTime.Now;

                    // Cập nhật trạng thái Appointment
                    appointment.Status = 2; // Confirmed (Đã xác nhận & thanh toán)
                    appointment.PaidAmount += paidAmount;
                    appointment.UpdatedAt = DateTime.Now;

                    await _paymentRepo.UpdateAsync(payment);
                    await _appointmentRepo.UpdateAsync(appointment);
                    await _appointmentRepo.SaveChangesAsync();

                    _logger.LogInformation("[PaymentCallback] Payment SUCCESS for Appointment {AppointmentId}. Ref={Ref}",
                        appointment.AppointmentId, transactionRef);

                    // SignalR thông báo (nếu có)
                    try
                    {
                        await _hub.Clients.Group("Admins").SendAsync("AppointmentUpdated", new { AppointmentId = appointment.AppointmentId, Status = appointment.Status });
                        if (appointment.CustomerId != Guid.Empty)
                            await _hub.Clients.User(appointment.CustomerId.ToString()).SendAsync("AppointmentUpdated", new { AppointmentId = appointment.AppointmentId, Status = appointment.Status });
                    }
                    catch { }

                    return new PlaceOrderResponseDTO
                    {
                        Success = true,
                        Message = "Thanh toán lịch hẹn thành công.",
                        OrderId = appointment.AppointmentId,
                        FinalAmount = appointment.Total
                    };
                }
                else
                {
                    payment.Status = 3; // Failed
                    await _paymentRepo.UpdateAsync(payment);
                    await _paymentRepo.SaveChangesAsync();

                    return new PlaceOrderResponseDTO
                    {
                        Success = false,
                        Message = "Thanh toán lịch hẹn thất bại.",
                        OrderId = appointment.AppointmentId
                    };
                }
            }

            Order? order = payment.OrderId.HasValue
                ? await _orderRepo.GetOrderWithDetailsByIdAsync(payment.OrderId.Value)
                : null;

            if (order == null)
            {
                _logger.LogError("[PaymentCallback] Order not found for PaymentId: {PaymentId}", payment.PaymentId);
                return new PlaceOrderResponseDTO
                {
                    Success = false,
                    Message = "Order not found for this payment."
                };
            }

            // ── 3. Handle FAILED payment ────────────────────────────────
            if (!isSuccess)
            {
                payment.Status = 3;  // Failed
                payment.ResponseCode = responseCode;
                payment.GatewayTransactionNo = gatewayTransactionNo;
                payment.RawResponse = rawResponse;
                payment.UpdatedAt = DateTime.Now;

                order.PaymentStatus = 3;  // Failed
                order.Status = 0;         // Cancelled
                order.UpdateAt = DateTime.Now;

                await _paymentRepo.UpdatePaymentAsync(payment);
                await _orderRepo.UpdateOrderAsync(order);
                await _orderRepo.SaveChangesAsync();

                _logger.LogInformation("[PaymentCallback] Payment FAILED for order {OrderId}. TransactionRef={Ref}, ResponseCode={Code}",
                    order.OrderId, transactionRef, responseCode);

                // Notify via SignalR
                try
                {
                    await _hub.Clients.Group("Admins").SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                    if (order.CustomerId != Guid.Empty)
                        await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                }
                catch { }

                return new PlaceOrderResponseDTO
                {
                    Success = false,
                    Message = "Payment was not successful. The order has been cancelled.",
                    OrderId = order.OrderId
                };
            }

            // ── 4. Handle SUCCESSFUL payment ────────────────────────────
            await using var tx = await _orderRepo.BeginTransactionAsync();
            try
            {
                // ── 4a. Re-check inventory availability ─────────────────
                var productIds = order.OrderDetails.Select(d => d.ProductId).Distinct().ToList();
                var inventories = await _inventoryRepo.GetInventoriesByProductIdsAsync(productIds);

                bool inventoryInsufficient = false;
                string insufficientProduct = string.Empty;

                foreach (var detail in order.OrderDetails)
                {
                    var inv = inventories.FirstOrDefault(i => i.ProductId == detail.ProductId);
                    if (inv == null || inv.QuantityAvailable < detail.Quantity)
                    {
                        inventoryInsufficient = true;
                        insufficientProduct = inv?.Product?.ProductName ?? detail.ProductId.ToString();
                        break;
                    }
                }

                if (inventoryInsufficient)
                {
                    payment.Status = 3;  // Failed (can't fulfill)
                    payment.ResponseCode = responseCode;
                    payment.GatewayTransactionNo = gatewayTransactionNo;
                    payment.PaidAmount = paidAmount;
                    payment.PaidAt = DateTime.Now;
                    payment.RawResponse = rawResponse;
                    payment.UpdatedAt = DateTime.Now;

                    order.PaymentStatus = 4;  // RefundRequired
                    order.Status = 0;         // Cancelled
                    order.UpdateAt = DateTime.Now;

                    await _paymentRepo.UpdatePaymentAsync(payment);
                    await _orderRepo.UpdateOrderAsync(order);
                    await _orderRepo.SaveChangesAsync();
                    await tx.CommitAsync();

                    _logger.LogWarning(
                        "[PaymentCallback] Payment SUCCESS but inventory insufficient for product '{Product}' in order {OrderId}. " +
                        "Order marked for REFUND. TransactionRef={Ref}, GatewayTxnNo={TxnNo}",
                        insufficientProduct, order.OrderId, transactionRef, gatewayTransactionNo);

                    try
                    {
                        await _hub.Clients.Group("Admins").SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                        if (order.CustomerId != Guid.Empty)
                            await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                    }
                    catch { }

                    return new PlaceOrderResponseDTO
                    {
                        Success = false,
                        Message = $"Payment received but product '{insufficientProduct}' is out of stock. A refund will be processed.",
                        OrderId = order.OrderId
                    };
                }

                // ── 4b. Reserve inventory ────────────────────────────────
                foreach (var detail in order.OrderDetails)
                {
                    var inv = inventories.First(i => i.ProductId == detail.ProductId);
                    inv.QuantityReserved += detail.Quantity;
                    inv.QuantityAvailable -= detail.Quantity;
                    inv.LastUpdated = DateTime.Now;
                }

                // ── 4c. Update Payment ──────────────────────────────────
                payment.Status = 2;  // Success
                payment.GatewayTransactionNo = gatewayTransactionNo;
                payment.ResponseCode = responseCode;
                payment.BankCode = bankCode;
                payment.PaidAmount = paidAmount;
                payment.PaidAt = DateTime.Now;
                payment.RawResponse = rawResponse;
                payment.UpdatedAt = DateTime.Now;

                // ── 4d. Update Order ────────────────────────────────────
                order.PaymentStatus = 2;  // Paid
                order.Status = 2;         // Confirmed
                order.UpdateAt = DateTime.Now;

                await _paymentRepo.UpdatePaymentAsync(payment);
                await _orderRepo.UpdateOrderAsync(order);
                await _orderRepo.SaveChangesAsync();
                await tx.CommitAsync();

                _logger.LogInformation(
                    "[PaymentCallback] Payment SUCCESS for order {OrderId}. Inventory deducted. " +
                    "TransactionRef={Ref}, GatewayTxnNo={TxnNo}, Amount={Amount}",
                    order.OrderId, transactionRef, gatewayTransactionNo, paidAmount);

                // Notify via SignalR
                try
                {
                    await _hub.Clients.Group("Admins").SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                    if (order.CustomerId != Guid.Empty)
                        await _hub.Clients.User(order.CustomerId.ToString()).SendAsync("OrderUpdated", new { OrderId = order.OrderId, Status = order.Status });
                }
                catch { }

                return new PlaceOrderResponseDTO
                {
                    Success = true,
                    Message = "Payment confirmed and order is being processed.",
                    OrderId = order.OrderId,
                    FinalAmount = order.TotalAmount
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<AvailableVoucherDTO>> GetAvailableVouchersAsync(Guid customerId, decimal orderAmount)
        {
            return await _voucherRepo.GetAvailableVouchersForCustomerAsync(customerId, orderAmount);
        }
    }
}
