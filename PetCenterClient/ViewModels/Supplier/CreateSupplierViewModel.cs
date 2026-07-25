using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.Supplier
{
    public class CreateSupplierViewModel
    {
        public Guid SupplierId { get; set; }

        [RegularExpression(
    @"^([0-9]{10}|[0-9]{13})$",
    ErrorMessage = "Tax identification number must be either 10 digits or 13 digits.")]
        [StringLength(
    13,
    ErrorMessage = "Tax identification number cannot exceed 13 characters.")]
        public string? TaxId { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(
            50,
            ErrorMessage = "Supplier name cannot exceed 50 characters")]
        public string SupplierName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(
            255,
            ErrorMessage = "Email cannot exceed 255 characters")]
        public string SupplierEmail { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
            @"^[0-9]{10,11}$",
            ErrorMessage = "Phone must be 10-11 digits")]
        public string SupplierPhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(
            200,
            ErrorMessage = "Address cannot exceed 200 characters")]
        public string SupplierAddress { get; set; } = null!;

        [StringLength(
            200,
            ErrorMessage = "Contact person cannot exceed 200 characters")]
        public string? ContactPerson { get; set; }

        [StringLength(
            200,
            ErrorMessage = "Supplier description cannot exceed 200 characters")]
        public string? SupplierDescription { get; set; }
    }
}
