using System;
using System.Collections.Generic;

namespace ImportAPI.Models;

public partial class Supplier
{
    public Guid SupplierId { get; set; }

    public string TaxId { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public string SupplierEmail { get; set; } = null!;

    public string SupplierPhoneNumber { get; set; } = null!;

    public string SupplierAddress { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? SupplierDescription { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ImportStock> ImportStocks { get; set; } = new List<ImportStock>();
}
