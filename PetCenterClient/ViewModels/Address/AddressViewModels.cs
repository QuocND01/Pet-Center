using System;
using System.Linq;
using System.Collections.Generic;

namespace PetCenterClient.ViewModels
{
    // Dùng ? client ð? hi?n th? ð?a ch?
    public class ReadAddressViewModel
    {
        public Guid AddressId { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? AddressDetails { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        public string FullAddress => string.Join(", ", new[] { AddressDetails, Ward, District, Province }.Where(s => !string.IsNullOrEmpty(s)));
    }

    // Dùng ð? g?i d? li?u t? form (thêm / s?a)
    public class MutateAddressViewModel
    {
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? AddressDetails { get; set; }
        public bool IsDefault { get; set; }
    }

    // Các lo?i model trý?c ðây có h?u t? DTO — gi? tên ð? gi?m thay ð?i phía client
    public class AddressResponseDTO
    {
        public Guid AddressId { get; set; }
        public Guid CustomerId { get; set; }
        public string AddressDetails { get; set; } = null!;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class AddressCreateDTO
    {
        public Guid CustomerId { get; set; }
        public string AddressDetails { get; set; } = null!;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public bool IsDefault { get; set; }
    }
}
