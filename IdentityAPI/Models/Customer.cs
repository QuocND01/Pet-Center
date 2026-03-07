using System;
using System.Collections.Generic;

namespace IdentityAPI.Models;

public partial class Customer
{
    public Guid CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateOnly? BirthDay { get; set; }

    public string Gender { get; set; } = null!;

    public bool? EmailVerified { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
