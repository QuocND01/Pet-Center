namespace PetCenterClient.DTOs
{
    // Dùng để NHẬN dữ liệu từ API về và hiển thị lên màn hình
    public class ReadAddressViewModel
    {
        public Guid AddressId { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? AddressDetails { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        // Hàm gộp chuỗi giúp UI hiển thị một dòng địa chỉ dài đẹp mắt
        public string FullAddress => string.Join(", ", new[] { AddressDetails, Ward, District, Province }.Where(s => !string.IsNullOrEmpty(s)));
    }

    // Dùng để GỬI dữ liệu từ Form (Thêm/Sửa) xuống API
    public class MutateAddressViewModel
    {
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? AddressDetails { get; set; }
        public bool IsDefault { get; set; }
    }
}