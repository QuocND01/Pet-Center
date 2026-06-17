using System;
using System.Collections.Generic;

namespace PetCenterAPI.DTOs.Requests.Order
{
    public class OrderRequestDTO
    {
        // 1. DTO cho danh sách đơn hàng
        public class ReadOrderListDTO
        {
            public Guid OrderId { get; set; }
            public string CustomerName { get; set; } = null!;
            public string PhoneNumber { get; set; } = null!;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public int Status { get; set; }
            public string PaymentMethod { get; set; } = null!;
            public int PaymentStatus { get; set; }
            public string AddressSnapshot { get; set; } = null!;
        }

        // 2. DTO cho chi tiết đơn hàng
        public class ReadOrderDetailDTO
        {
            public Guid OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal? DiscountAmount { get; set; }
            public int Status { get; set; }
            public string PaymentMethod { get; set; } = null!;
            public int PaymentStatus { get; set; }
            public string AddressSnapshot { get; set; } = null!;

            // Customer Info
            public string CustomerName { get; set; } = null!;
            public string PhoneNumber { get; set; } = null!;
            public string Email { get; set; } = null!;

            // Order Items
            public List<ReadOrderItemDTO> OrderItems { get; set; } = new();
        }

        // 3. DTO con cho từng item sản phẩm trong đơn hàng
        public class ReadOrderItemDTO
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public string ProductCategory { get; set; } = null!;
            public string ProductBrand { get; set; } = null!;
            public string ProductImage { get; set; } = null!;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal SubTotal => Quantity * UnitPrice;
        }
    }
}