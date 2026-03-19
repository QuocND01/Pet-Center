using System;
using System.Collections.Generic;

namespace PromotionAPI.Models;

public partial class CustomerVoucher
{
    public Guid CustomerId { get; set; }

    public Guid VoucherId { get; set; }

    public bool? IsUsed { get; set; }

    public virtual Voucher Voucher { get; set; } = null!;
}
