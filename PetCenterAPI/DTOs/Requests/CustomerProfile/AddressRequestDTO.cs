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
            public string? Province { get; set; }
            public string? District { get; set; }
            public string? Ward { get; set; }
            public string? AddressDetails { get; set; }
            public bool IsDefault { get; set; }
        }
    }
}