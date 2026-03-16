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

            // Apply voucher
            if (dto.VoucherId.HasValue)
            {
                var cv = await _voucherContext.CustomerVouchers
                    .Include(x => x.Voucher)
                    .FirstOrDefaultAsync(x =>
                        x.CustomerId == dto.CustomerId &&
                        x.VoucherId == dto.VoucherId.Value &&
                        x.IsUsed != true);   // FIX: != true matches both false AND null

                if (cv?.Voucher != null)
                {
                    var v = cv.Voucher;
                    bool valid = v.IsActive == true
                               && (v.ExpiredDate == null || v.ExpiredDate > DateTime.UtcNow)
                               && (v.MinOrderAmount == null || totalAmount >= v.MinOrderAmount);

                    if (valid && v.DiscountPercent.HasValue)
                    {
                        discountAmount = totalAmount * v.DiscountPercent.Value / 100m;
                        if (v.MaxDiscountAmount.HasValue && discountAmount > v.MaxDiscountAmount.Value)
                            discountAmount = v.MaxDiscountAmount.Value;
                    }
                }
            }

            decimal finalAmount = totalAmount - discountAmount;

            // Create Order
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

            // Create OrderDetails
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

            // Mark voucher used
            if (dto.VoucherId.HasValue && discountAmount > 0)
            {
                var cv = await _voucherContext.CustomerVouchers
                    .FirstOrDefaultAsync(x =>
                        x.CustomerId == dto.CustomerId &&
                        x.VoucherId == dto.VoucherId.Value);
                if (cv != null)
                {
                    cv.IsUsed = true;
                    await _voucherContext.SaveChangesAsync();
                }
            }

            // Clear only checked-out products from cart
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

            // FIX: IsUsed != true  →  matches NULL và false (cả hai đều là "chưa dùng")
            var list = await _voucherContext.CustomerVouchers
                .Include(cv => cv.Voucher)
                .Where(cv =>
                    cv.CustomerId == customerId &&
                    cv.IsUsed != true &&                                        // FIX: != true thay vì == false
                    cv.Voucher.IsActive == true &&
                    (cv.Voucher.ExpiredDate == null || cv.Voucher.ExpiredDate > now) &&
                    (cv.Voucher.MinOrderAmount == null || cv.Voucher.MinOrderAmount <= orderAmount))
                .Select(cv => new CustomerVoucherDTO
                {
                    VoucherId = cv.VoucherId,
                    Code = cv.Voucher.Code,
                    Description = cv.Voucher.Description,
                    DiscountPercent = cv.Voucher.DiscountPercent,
                    MinOrderAmount = cv.Voucher.MinOrderAmount,
                    MaxDiscountAmount = cv.Voucher.MaxDiscountAmount,
                    ExpiredDate = cv.Voucher.ExpiredDate,
                    IsUsed = cv.IsUsed ?? false
                })
                .ToListAsync();

            return list;
        }

        public async Task<VoucherValidateResponseDTO> ValidateVoucherAsync(VoucherValidateRequestDTO dto)
        {
            var now = DateTime.UtcNow;

            // FIX: IsUsed != true
            var cv = await _voucherContext.CustomerVouchers
                .Include(x => x.Voucher)
                .FirstOrDefaultAsync(x =>
                    x.CustomerId == dto.CustomerId &&
                    x.Voucher.Code == dto.Code &&
                    x.IsUsed != true);   // FIX

            if (cv == null)
                return new VoucherValidateResponseDTO { Valid = false, Message = "Voucher not found or already used." };

            var v = cv.Voucher;

            if (v.IsActive != true)
                return new VoucherValidateResponseDTO { Valid = false, Message = "Voucher is inactive." };

            if (v.ExpiredDate.HasValue && v.ExpiredDate < now)
                return new VoucherValidateResponseDTO { Valid = false, Message = "Voucher has expired." };

            if (v.MinOrderAmount.HasValue && dto.OrderAmount < v.MinOrderAmount.Value)
                return new VoucherValidateResponseDTO
                {
                    Valid = false,
                    Message = $"Minimum order amount is {v.MinOrderAmount.Value:N0} ₫."
                };

            decimal disc = 0;
            if (v.DiscountPercent.HasValue)
            {
                disc = dto.OrderAmount * v.DiscountPercent.Value / 100m;
                if (v.MaxDiscountAmount.HasValue && disc > v.MaxDiscountAmount.Value)
                    disc = v.MaxDiscountAmount.Value;
            }

            return new VoucherValidateResponseDTO
            {
                Valid = true,
                Message = "Voucher applied!",
                VoucherId = v.VoucherId,
                Code = v.Code,
                DiscountPercent = v.DiscountPercent,
                MaxDiscountAmount = v.MaxDiscountAmount,
                DiscountAmount = disc,
                FinalAmount = dto.OrderAmount - disc,
                Description = v.Description
            };
        }
    }
}
