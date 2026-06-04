using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class Customer
{
    public Guid CustomerId { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? BirthDay { get; set; }

    public string? Gender { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsVerified { get; set; }

    public string? PasswordHash { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();

    public virtual ICollection<VetFeedback> VetFeedbacks { get; set; } = new List<VetFeedback>();
}
