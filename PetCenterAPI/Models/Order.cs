using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class Order
{
    public Guid OrderId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? StaffId { get; set; }

    public Guid AddressId { get; set; }

    public string AddressSnapshot { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int Status { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public int PaymentStatus { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();

    public virtual Staff? Staff { get; set; }
}
