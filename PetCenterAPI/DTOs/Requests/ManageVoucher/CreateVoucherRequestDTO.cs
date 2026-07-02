using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.ManageVoucher
{
    public class CreateVoucherRequestDTO
    {
        [Required(ErrorMessage = "Code is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Code must be 2–50 characters.")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Code must be uppercase letters and numbers only.")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Discount percent is required.")]
        [Range(1, 80, ErrorMessage = "Discount must be between 1% and 80%.")]
        public int DiscountPercent { get; set; }

        [StringLength(100, ErrorMessage = "Description must not exceed 100 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Min order amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Min order amount must be > 0.")]
        public decimal MinOrderAmount { get; set; }

        [Required(ErrorMessage = "Max discount amount is required.")]
        [Range(1, 50_000_000, ErrorMessage = "Max discount must be between 1₫ and 50,000,000₫.")]
        public decimal MaxDiscountAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be ≥ 1 if provided.")]
        public int? UseageLimit { get; set; }

        public DateTime? ExpiredDate { get; set; }
    }
}
