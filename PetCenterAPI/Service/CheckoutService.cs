using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs.Requests.Order;
using PetCenterAPI.DTOs.Responses.Order;
using PetCenterAPI.Models;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class CheckoutService : ICheckoutService
    {
        private readonly PetCenterContext _db;

        public CheckoutService(PetCenterContext db)
        {
            _db = db;
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
                    throw new InvalidOperationException("Địa chỉ không hợp lệ hoặc không thuộc về tài khoản này.");

                var addrParts = new[] { address.AddressDetails, address.Ward, address.District, address.Province };
                var addressSnapshot = string.Join(", ", addrParts.Where(s => !string.IsNullOrEmpty(s)));

                // ── 2. Compute subtotal ──────────────────────────────────────
                var subtotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity);

                // ── 3. Voucher discount ──────────────────────────────────────
                decimal discountAmount = 0;
                if (dto.VoucherId.HasValue)
                {
                    var alreadyUsed = await _db.CustomerVouchers
                        .AnyAsync(cv => cv.CustomerId == dto.CustomerId
                                     && cv.VoucherId == dto.VoucherId.Value
                                     && cv.IsUsed == true);
                    if (alreadyUsed)
                        throw new InvalidOperationException("Bạn đã sử dụng voucher này rồi.");

                    var voucher = await _db.Vouchers
                        .FirstOrDefaultAsync(v => v.VoucherId == dto.VoucherId.Value);

                    if (voucher == null || voucher.IsActive != true)
                        throw new InvalidOperationException("Voucher không hợp lệ hoặc đã bị vô hiệu hóa.");

                    if (voucher.ExpiredDate.HasValue && voucher.ExpiredDate.Value < DateTime.Now)
                        throw new InvalidOperationException("Voucher đã hết hạn.");

                    if (voucher.MinOrderAmount.HasValue && subtotal < voucher.MinOrderAmount.Value)
                        throw new InvalidOperationException($"Đơn hàng tối thiểu {voucher.MinOrderAmount.Value:N0} ₫ để áp dụng voucher này.");

                    if (voucher.UseageLimit.HasValue)
                    {
                        var usedCount = await _db.CustomerVouchers
                            .CountAsync(cv => cv.VoucherId == dto.VoucherId.Value && cv.IsUsed == true);
                        if (usedCount >= voucher.UseageLimit.Value)
                            throw new InvalidOperationException("Voucher đã đạt giới hạn số lượt sử dụng.");
                    }

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
                        throw new InvalidOperationException("Không tìm thấy thông tin tồn kho cho sản phẩm.");

                    var available = inv.QuantityAvailable - inv.QuantityReserved;
                    if (available < item.Quantity)
                        throw new InvalidOperationException(
                            $"Sản phẩm '{inv.Product.ProductName}' không đủ số lượng tồn. " +
                            $"(Còn: {available}, Yêu cầu: {item.Quantity})");
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
                    inv.LastUpdated = DateTime.Now;
                }

                // ── 8. Mark voucher used ─────────────────────────────────────
                if (dto.VoucherId.HasValue)
                {
                    _db.CustomerVouchers.Add(new CustomerVoucher
                    {
                        CustomerId = dto.CustomerId,
                        VoucherId = dto.VoucherId.Value,
                        IsUsed = true
                    });
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

                return new PlaceOrderResponseDTO
                {
                    Success = true,
                    Message = "Đặt hàng thành công!",
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
