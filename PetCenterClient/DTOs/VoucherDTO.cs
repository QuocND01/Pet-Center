namespace PetCenterClient.DTOs
{
    public class VoucherDto
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = null!;
        public int? DiscountPercent { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? UseageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime? CreateAt { get; set; }
        public string? Description { get; set; }
    }

    // ── CREATE REQUEST ────────────────────────────────────────
    public class CreateVoucherDto
    {
        public string Code { get; set; } = null!;
        public int DiscountPercent { get; set; }
        public string? Description { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public int? UseageLimit { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ── UPDATE REQUEST ────────────────────────────────────────
    public class UpdateVoucherDto
    {
        public string Code { get; set; } = null!;
        public int DiscountPercent { get; set; }
        public string? Description { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public int? UseageLimit { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsActive { get; set; }
    }

    // ── TOGGLE REQUEST ────────────────────────────────────────
    public class ToggleVoucherDto
    {
        public bool IsActive { get; set; }
    }
}
