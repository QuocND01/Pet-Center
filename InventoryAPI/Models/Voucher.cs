using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Voucher
{
    public Guid VoucherId { get; set; }

    public string Code { get; set; } = null!;

    public int? DiscountPercent { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? ExpiredDate { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int? UseageLimit { get; set; }

    public DateTime? CreateAt { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();
}
