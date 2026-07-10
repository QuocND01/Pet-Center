using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.Models;

public partial class Order
{
    public Guid OrderId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? StaffId { get; set; }

    public Guid AddressId { get; set; }

    [Required]
    public string AddressSnapshot { get; set; } = null!;

    [DataType(DataType.DateTime)]
    public DateTime? OrderDate { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? DeliveredDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? DiscountAmount { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int Status { get; set; }

    public int PaymentStatus { get; set; }

    [Required]
    [StringLength(100)]
    public string PaymentMethod { get; set; } = null!;

    public virtual Address Address { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();

    public virtual Staff? Staff { get; set; }
}
