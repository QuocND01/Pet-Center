using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal Amount { get; set; }

    public int Status { get; set; }

    public string? TransactionRef { get; set; }

    public string? GatewayTransactionNo { get; set; }

    public string? ResponseCode { get; set; }

    public string? BankCode { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? RawResponse { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
