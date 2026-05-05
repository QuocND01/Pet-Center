namespace CustomerAPI.DTOs.Address
{
    public class ViewAddressDto
    {
        public Guid AddressId { get; set; }

        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? AddressDetails { get; set; }

        public string? ReceiverName { get; set; }
        public string? Phone { get; set; }

        public bool? IsDefault { get; set; }
    }
    public class CreateAddressDto
    {
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? AddressDetails { get; set; }

        public string? ReceiverName { get; set; }
        public string? Phone { get; set; }

        public bool? IsDefault { get; set; }
    }

    public class UpdateAddressDto : CreateAddressDto
    {
    }
}
