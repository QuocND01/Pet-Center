using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.Models;

public partial class Address
{
    public Guid AddressId { get; set; }

    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string? Province { get; set; }

    [Required]
    [StringLength(100)]
    public string? District { get; set; }

    [StringLength(100)]
    public string? Ward { get; set; }

    [Required]
    [StringLength(300)]
    public string? AddressDetails { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
