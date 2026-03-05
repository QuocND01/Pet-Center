using System;
using System.Collections.Generic;

namespace AddressAPI.Models;

public partial class ProductFeedback
{
    public Guid FeedbackId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid ProductId { get; set; }

    public Guid OrderId { get; set; }

    public Guid? StaffId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public string? Reply { get; set; }

    public DateTime? ReplyDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsVisible { get; set; }

    public bool? IsActive { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Staff? Staff { get; set; }
}
