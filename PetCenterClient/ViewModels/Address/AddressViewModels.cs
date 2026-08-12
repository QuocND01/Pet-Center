using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels
{
    // Models used to display and mutate address data
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

    // Model used to receive data from the form (create/edit)
    public class MutateAddressViewModel
    {
        [Required(ErrorMessage = "Province / City is required.")]
        [StringLength(200, ErrorMessage = "Province / City is too long.")]
        public string? Province { get; set; }

        [Required(ErrorMessage = "District is required.")]
        [StringLength(200, ErrorMessage = "District is too long.")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Ward is required.")]
        [StringLength(200, ErrorMessage = "Ward is too long.")]
        public string? Ward { get; set; }

        [Required(ErrorMessage = "Street & house number is required.")]
        [StringLength(500, ErrorMessage = "Address details is too long.")]
        public string? AddressDetails { get; set; }

        public bool IsDefault { get; set; }
    }

    // DTOs used by API client
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
