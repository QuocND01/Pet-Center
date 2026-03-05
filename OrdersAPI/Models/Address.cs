using System;
using System.Collections.Generic;

namespace OrdersAPI.Models;

public partial class Address
{
    public Guid AddressId { get; set; }

    public Guid CustomerId { get; set; }

    public string AddressDetails { get; set; } = null!;

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Ward { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
