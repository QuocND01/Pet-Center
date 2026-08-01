using System;
using System.Collections.Generic;
using System.Linq;

namespace PetCenterClient.ViewModels
{
    // ViewModel for Checkout page
    public class CheckoutViewModel
    {
        public List<CheckoutCartItemVM> SelectedItems { get; set; } = new();
        public List<AddressResponseDTO> Addresses { get; set; } = new();
        public List<PetCenterClient.DTOs.CustomerVoucherDTO> AvailableVouchers { get; set; } = new();
        public Guid CustomerId { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal SubTotal => SelectedItems.Sum(i => i.SubTotal);
    }

    public class CheckoutCartItemVM
    {
        public Guid CartDetailId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
    }
}
