using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PetCenterAPI.DTOs.Requests.CustomerProfile
{
    public class AddressRequestDTO
    {
        public class ReadAddressDTO
        {
            public Guid AddressId { get; set; }
            public string? Province { get; set; }
            public string? District { get; set; }
            public string? Ward { get; set; }
            public string? AddressDetails { get; set; }
            public bool IsDefault { get; set; }
            public bool IsActive { get; set; }

            // Thuộc tính gộp chuỗi để UI Client hiển thị cho đẹp
            public string FullAddress => string.Join(", ", new[] { AddressDetails, Ward, District, Province }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public class MutateAddressDTO
        {
            [Required(ErrorMessage = "Province is required")]
            [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
            public string? Province { get; set; }

            [Required(ErrorMessage = "District is required")]
            [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
            public string? District { get; set; }

            [StringLength(100, ErrorMessage = "Ward cannot exceed 100 characters")]
            public string? Ward { get; set; }

            [Required(ErrorMessage = "AddressDetails is required")]
            [StringLength(300, ErrorMessage = "AddressDetails cannot exceed 300 characters")]
            public string? AddressDetails { get; set; }

            public bool IsDefault { get; set; }
        }
    }
}