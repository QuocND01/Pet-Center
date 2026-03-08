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

// AddressCreateDTO.cs (Dùng khi thêm mới)
public class AddressCreateDTO
{
    public Guid CustomerId { get; set; }
    public string AddressDetails { get; set; } = null!;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public bool IsDefault { get; set; }
}