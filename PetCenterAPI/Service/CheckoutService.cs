using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using PetCenterAPI.Hubs;
using PetCenterAPI.DTOs.Requests.Order;
using PetCenterAPI.DTOs.Responses.Order;
using PetCenterAPI.Models;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class CheckoutService : ICheckoutService
    {
        private readonly PetCenterContext _db;
        private readonly IHubContext<AppHub> _hub;
        private readonly IVnPayService _vnPayService;
        private readonly IMoMoService _moMoService;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            PetCenterContext db,
            IHubContext<AppHub> hub,
            IVnPayService vnPayService,
            IMoMoService moMoService,
            ILogger<CheckoutService> logger)
        {
            _db = db;
            _hub = hub;
            _vnPayService = vnPayService;
            _moMoService = moMoService;
            _logger = logger;
        }

        public async Task<PlaceOrderResponseDTO> PlaceCodOrderAsync(PlaceCodOrderDTO dto)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // ── 1. Validate address ──────────────────────────────────────
                var address = await _db.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == dto.AddressId
                                           && a.CustomerId == dto.CustomerId
                                           && a.IsActive == true);

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
                    var alreadyUsed = await _db.CustomerVouchers
                        .AnyAsync(cv => cv.CustomerId == dto.CustomerId
                                     && cv.VoucherId == dto.VoucherId.Value
                                     && cv.IsUsed == true);
                    if (alreadyUsed)
                        throw new InvalidOperationException("You have already used this voucher.");

                    voucher = await _db.Vouchers
                        .FirstOrDefaultAsync(v => v.VoucherId == dto.VoucherId.Value);

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

                // ── 4. Check inventory ───────────────────────────────────────
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var inventories = await _db.Inventories
                    .Include(inv => inv.Product)
                    .Where(inv => productIds.Contains(inv.ProductId))
                    .ToListAsync();

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
                _db.Orders.Add(order);

                // ── 6. OrderDetails + Snapshots ──────────────────────────────
                var products = await _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                foreach (var item in dto.Items)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);
                    var detail = new OrderDetail
                    {
                        OrderDetailsId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ImportStockDetailsId = null
                    };
                    _db.OrderDetails.Add(detail);

                    _db.OrderProductSnapshots.Add(new OrderProductSnapshot
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
                    var customerVoucher = await _db.CustomerVouchers
                        .FirstOrDefaultAsync(cv => cv.CustomerId == dto.CustomerId
                                             && cv.VoucherId == dto.VoucherId.Value);

                    if (customerVoucher != null)
                    {
                        customerVoucher.IsUsed = true;
                    }
                    else
                    {
                        _db.CustomerVouchers.Add(new CustomerVoucher
                        {
                            CustomerId = dto.CustomerId,
                            VoucherId = dto.VoucherId.Value,
                            IsUsed = true
                        });
                    }

                    if (voucher.UseageLimit.HasValue)
                        voucher.UseageLimit = voucher.UseageLimit.Value - 1;
                }

                // ── 9. Clear cart ────────────────────────────────────────────
                var cart = await _db.Carts.FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);
                if (cart != null)
                {
                    await _db.CartDetails
                        .Where(cd => cd.CartId == cart.CartId)
                        .ExecuteDeleteAsync();
                }

                await _db.SaveChangesAsync();
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
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // ── 1. Validate address ──────────────────────────────────────
                var address = await _db.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == dto.AddressId
                                           && a.CustomerId == dto.CustomerId
                                           && a.IsActive == true);

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
                    var alreadyUsed = await _db.CustomerVouchers
                        .AnyAsync(cv => cv.CustomerId == dto.CustomerId
                                     && cv.VoucherId == dto.VoucherId.Value
                                     && cv.IsUsed == true);
                    if (alreadyUsed)
                        throw new InvalidOperationException("You have already used this voucher.");

                    voucher = await _db.Vouchers
                        .FirstOrDefaultAsync(v => v.VoucherId == dto.VoucherId.Value);

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

                // ── 4. Check inventory (availability only, NO reservation) ───
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var inventories = await _db.Inventories
                    .Include(inv => inv.Product)
                    .Where(inv => productIds.Contains(inv.ProductId))
                    .ToListAsync();

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
                _db.Orders.Add(order);

                // ── 7. OrderDetails + Snapshots ──────────────────────────────
                var products = await _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                foreach (var item in dto.Items)
                {
                    var product = products.First(p => p.ProductId == item.ProductId);
                    var detail = new OrderDetail
                    {
                        OrderDetailsId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ImportStockDetailsId = null
                    };
                    _db.OrderDetails.Add(detail);

                    _db.OrderProductSnapshots.Add(new OrderProductSnapshot
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
                _db.Payments.Add(payment);

                // ── 9. Mark voucher used + decrement usage limit ─────────────
                if (dto.VoucherId.HasValue && voucher != null)
                {
                    var customerVoucher = await _db.CustomerVouchers
                        .FirstOrDefaultAsync(cv => cv.CustomerId == dto.CustomerId
                                             && cv.VoucherId == dto.VoucherId.Value);

                    if (customerVoucher != null)
                    {
                        customerVoucher.IsUsed = true;
                    }
                    else
                    {
                        _db.CustomerVouchers.Add(new CustomerVoucher
                        {
                            CustomerId = dto.CustomerId,
                            VoucherId = dto.VoucherId.Value,
                            IsUsed = true
                        });
                    }

                    if (voucher.UseageLimit.HasValue)
                        voucher.UseageLimit = voucher.UseageLimit.Value - 1;
                }

                // ── 10. Clear cart ───────────────────────────────────────────
                var cart = await _db.Carts.FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);
                if (cart != null)
                {
                    await _db.CartDetails
                        .Where(cd => cd.CartId == cart.CartId)
                        .ExecuteDeleteAsync();
                }

                await _db.SaveChangesAsync();
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
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);

            if (payment == null)
            {
                _logger.LogWarning("[PaymentCallback] Payment not found for TransactionRef: {Ref}", transactionRef);
                return new PlaceOrderResponseDTO
                {
                    Success = false,
                    Message = "Payment record not found for the given transaction reference."
                };
            }

            // ── 2. Idempotency check ────────────────────────────────────
            if (payment.Status == 2 || payment.Status == 3)
            {
                _logger.LogInformation("[PaymentCallback] Payment {Ref} has already been processed (Status={Status}). Skipping.",
                    transactionRef, payment.Status);
                return new PlaceOrderResponseDTO
                {
                    Success = true,
                    Message = "This payment has already been processed.",
                    OrderId = payment.OrderId
                };
            }

            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == payment.OrderId);

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

                await _db.SaveChangesAsync();

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
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // ── 4a. Re-check inventory availability ─────────────────
                var productIds = order.OrderDetails.Select(d => d.ProductId).Distinct().ToList();
                var inventories = await _db.Inventories
                    .Include(inv => inv.Product)
                    .Where(inv => productIds.Contains(inv.ProductId))
                    .ToListAsync();

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
                    // Payment succeeded at gateway but we can't fulfill — mark for refund
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

                    await _db.SaveChangesAsync();
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

                // ── 4b. Deduct inventory directly (no Reserved involved) ─
                foreach (var detail in order.OrderDetails)
                {
                    var inv = inventories.First(i => i.ProductId == detail.ProductId);

                    int qtyBefore = inv.QuantityAvailable + inv.QuantityReserved;
                    inv.QuantityAvailable -= detail.Quantity;
                    int qtyAfter = inv.QuantityAvailable + inv.QuantityReserved;
                    inv.LastUpdated = DateTime.Now;

                    // Record inventory transaction
                    _db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        TransactionId = Guid.NewGuid(),
                        InventoryId = inv.InventoryId,
                        TransactionType = TransactionType.StockOut,
                        QuantityChange = -detail.Quantity,
                        QuantityBefore = qtyBefore,
                        QuantityAfter = qtyAfter,
                        CreatedAt = DateTime.Now,
                        CreatedBy = order.CustomerId,
                        ReferenceId = order.OrderId,
                        ReferenceType = ReferenceType.Order,
                        ImportStockDetailId = null,
                        Note = $"Online payment stock deduction ({payment.PaymentMethod})"
                    });
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

                await _db.SaveChangesAsync();
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
            var now = DateTime.Now;

            var usedVoucherIds = await _db.CustomerVouchers
                .Where(cv => cv.CustomerId == customerId && cv.IsUsed == true)
                .Select(cv => cv.VoucherId)
                .ToListAsync();

            return await _db.Vouchers
                .Where(v => v.IsActive == true
                         && (v.ExpiredDate == null || v.ExpiredDate >= now)
                         && (v.MinOrderAmount == null || v.MinOrderAmount <= orderAmount)
                         && (v.UseageLimit == null || v.UseageLimit > 0)
                         && !usedVoucherIds.Contains(v.VoucherId))
                .Select(v => new AvailableVoucherDTO
                {
                    VoucherId = v.VoucherId,
                    Code = v.Code,
                    Description = v.Description,
                    DiscountPercent = v.DiscountPercent,
                    MinOrderAmount = v.MinOrderAmount,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    ExpiredDate = v.ExpiredDate
                })
                .ToListAsync();
        }
    }
}
