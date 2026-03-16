using Microsoft.EntityFrameworkCore;
using OrdersAPI.DTOs;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Service
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderDetailRepository _detailRepo;
        private readonly ICartRepository _cartRepo;
        private readonly PetCenterVoucherServiceContext _voucherContext;

        public CheckoutService(
            IOrderRepository orderRepo,
            IOrderDetailRepository detailRepo,
            ICartRepository cartRepo,
            PetCenterVoucherServiceContext voucherContext)
        {
            _orderRepo = orderRepo;
            _detailRepo = detailRepo;
            _cartRepo = cartRepo;
            _voucherContext = voucherContext;
        }

        public async Task<CheckoutResponseDTO> ProcessCheckoutAsync(CheckoutRequestDTO dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return new CheckoutResponseDTO { Success = false, Message = "No items selected." };

            decimal totalAmount = dto.Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal discountAmount = 0;

            // Apply voucher nếu customer chọn
            if (dto.VoucherId.HasValue)
            {
                var voucher = await _voucherContext.Vouchers
                    .FirstOrDefaultAsync(v => v.VoucherId == dto.VoucherId.Value
                                           && v.IsActive == true);

                if (voucher != null)
                {
                    var now = DateTime.UtcNow;
                    bool valid = (voucher.ExpiredDate == null || voucher.ExpiredDate > now)
                              && (voucher.MinOrderAmount == null || totalAmount >= voucher.MinOrderAmount);

                    if (valid && voucher.DiscountPercent.HasValue)
                    {
                        discountAmount = totalAmount * voucher.DiscountPercent.Value / 100m;
                        if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
                            discountAmount = voucher.MaxDiscountAmount.Value;
                    }
                }
            }

            decimal finalAmount = totalAmount - discountAmount;

            // Tạo Order
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = dto.CustomerId,
                AddressId = dto.AddressId,
                AddressSnapshot = dto.AddressSnapshot,
                TotalAmount = finalAmount,
                DiscountAmount = discountAmount,
                OrderDate = DateTime.Now,
                Status = 1
            };

            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();

            // Tạo OrderDetails
            foreach (var item in dto.Items)
            {
                await _detailRepo.AddAsync(new OrderDetail
                {
                    OrderDetailId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }
            await _detailRepo.SaveChangesAsync();

            // Ghi nhận customer đã dùng voucher vào CustomerVouchers
            if (dto.VoucherId.HasValue && discountAmount > 0)
            {
                var existingCv = await _voucherContext.CustomerVouchers
                    .FirstOrDefaultAsync(cv => cv.CustomerId == dto.CustomerId
                                            && cv.VoucherId == dto.VoucherId.Value);

                if (existingCv != null)
                {
                    // Đã có record (do lần trước gán sẵn) → đánh dấu đã dùng
                    existingCv.IsUsed = true;
                }
                else
                {
                    // Chưa có record → tạo mới với IsUsed = true để ghi nhận đã dùng
                    _voucherContext.CustomerVouchers.Add(new CustomerVoucher
                    {
                        CustomerId = dto.CustomerId,
                        VoucherId = dto.VoucherId.Value,
                        IsUsed = true
                    });
                }

                await _voucherContext.SaveChangesAsync();
            }

            // Xóa sản phẩm đã checkout khỏi Cart
            var cart = await _cartRepo.GetCartByCustomerIdAsync(dto.CustomerId);
            if (cart != null)
            {
                var checkedIds = dto.Items.Select(i => i.ProductId).ToHashSet();
                foreach (var cd in cart.CartDetails.ToList())
                {
                    if (checkedIds.Contains(cd.ProductId))
                        await _cartRepo.DeleteCartDetailAsync(cd);
                }
                await _cartRepo.SaveChangesAsync();
            }

            return new CheckoutResponseDTO
            {
                Success = true,
                Message = "Order placed successfully!",
                OrderId = order.OrderId,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount
            };
        }

        public async Task<List<CustomerVoucherDTO>> GetAvailableVouchersAsync(Guid customerId, decimal orderAmount)
        {
            var now = DateTime.UtcNow;

            // Lấy danh sách VoucherId mà customer này đã dùng rồi
            var usedVoucherIds = await _voucherContext.CustomerVouchers
                .Where(cv => cv.CustomerId == customerId && cv.IsUsed == true)
                .Select(cv => cv.VoucherId)
                .ToListAsync();

            // Lấy TẤT CẢ voucher active, chưa hết hạn, đủ điều kiện đơn hàng
            // và loại trừ những voucher customer đã dùng
            var vouchers = await _voucherContext.Vouchers
                .Where(v => v.IsActive == true
                         && !usedVoucherIds.Contains(v.VoucherId)
                         && (v.ExpiredDate == null || v.ExpiredDate > now)
                         && (v.MinOrderAmount == null || v.MinOrderAmount <= orderAmount))
                .ToListAsync();

            return vouchers.Select(v => new CustomerVoucherDTO
            {
                VoucherId = v.VoucherId,
                Code = v.Code,
                Description = v.Description,
                DiscountPercent = v.DiscountPercent,
                MinOrderAmount = v.MinOrderAmount,
                MaxDiscountAmount = v.MaxDiscountAmount,
                ExpiredDate = v.ExpiredDate,
                IsUsed = false
            }).ToList();
        }

        public async Task<VoucherValidateResponseDTO> ValidateVoucherAsync(VoucherValidateRequestDTO dto)
        {
            var now = DateTime.UtcNow;

            // Tìm voucher theo code
            var voucher = await _voucherContext.Vouchers
                .FirstOrDefaultAsync(v => v.Code == dto.Code && v.IsActive == true);

            if (voucher == null)
                return new VoucherValidateResponseDTO { Valid = false, Message = "Voucher not found or inactive." };

            // Kiểm tra customer đã dùng voucher này chưa
            bool alreadyUsed = await _voucherContext.CustomerVouchers
                .AnyAsync(cv => cv.CustomerId == dto.CustomerId
                             && cv.VoucherId == voucher.VoucherId
                             && cv.IsUsed == true);

            if (alreadyUsed)
                return new VoucherValidateResponseDTO { Valid = false, Message = "You have already used this voucher." };

            if (voucher.ExpiredDate.HasValue && voucher.ExpiredDate < now)
                return new VoucherValidateResponseDTO { Valid = false, Message = "Voucher has expired." };

            if (voucher.MinOrderAmount.HasValue && dto.OrderAmount < voucher.MinOrderAmount.Value)
                return new VoucherValidateResponseDTO
                {
                    Valid = false,
                    Message = $"Minimum order amount is {voucher.MinOrderAmount.Value:N0} ₫."
                };

            decimal disc = 0;
            if (voucher.DiscountPercent.HasValue)
            {
                disc = dto.OrderAmount * voucher.DiscountPercent.Value / 100m;
                if (voucher.MaxDiscountAmount.HasValue && disc > voucher.MaxDiscountAmount.Value)
                    disc = voucher.MaxDiscountAmount.Value;
            }

            return new VoucherValidateResponseDTO
            {
                Valid = true,
                Message = "Voucher applied!",
                VoucherId = voucher.VoucherId,
                Code = voucher.Code,
                DiscountPercent = voucher.DiscountPercent,
                MaxDiscountAmount = voucher.MaxDiscountAmount,
                DiscountAmount = disc,
                FinalAmount = dto.OrderAmount - disc,
                Description = voucher.Description
            };
        }
    }
}