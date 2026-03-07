using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.DTOs
{
    public class ReadSupplierDto
    {
        public Guid SupplierId { get; set; }
        public string? TaxId { get; set; }
        public string SupplierName { get; set; } = null!;
        public string SupplierEmail { get; set; } = null!;
        public string SupplierPhoneNumber { get; set; } = null!;
        public string SupplierAddress { get; set; } = null!;
        public string? ContactPersion { get; set; }
        public string? SupplierDescription { get; set; }
        public bool IsActive { get; set; }
    }
    public class CreateSupplierDto
    {
        [StringLength(50)]
        public string? TaxId { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(150)]
        public string SupplierName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        public string SupplierEmail { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{9,15}$", ErrorMessage = "Phone must be 9-15 digits")]
        public string SupplierPhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(100)]
        public string SupplierAddress { get; set; } = null!;
        [StringLength(50)]
        public string? ContactPersion { get; set; }
        [StringLength(500)]
        public string? SupplierDescription { get; set; }
    }
    public class UpdateSupplierDto
    {
        [Required]
        public Guid SupplierId { get; set; }

        [StringLength(50)]
        public string? TaxId { get; set; }

        [Required]
        [StringLength(150)]
        public string SupplierName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string SupplierEmail { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[0-9]{9,15}$", ErrorMessage = "Phone must be 9-15 digits")]
        public string SupplierPhoneNumber { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string SupplierAddress { get; set; } = null!;

        [StringLength(50)]
        public string? ContactPersion { get; set; }

        [StringLength(500)]
        public string? SupplierDescription { get; set; }
    }
}
