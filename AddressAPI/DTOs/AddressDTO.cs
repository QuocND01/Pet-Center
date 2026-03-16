// File: AddressAPI/DTOs/AddressDTO.cs

public class AddressResponseDTO
{
    public Guid AddressId { get; set; }
    public Guid CustomerId { get; set; }
    public string AddressDetails { get; set; } = null!;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }   // ← thêm field này
}

public class AddressCreateDTO
{
    public Guid CustomerId { get; set; }
    public string AddressDetails { get; set; } = null!;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public bool? IsDefault { get; set; }
}