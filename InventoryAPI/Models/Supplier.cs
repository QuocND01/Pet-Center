using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Models;

public partial class Supplier
{
    [Key]
    [Column("SupplierID")]
    public Guid SupplierId { get; set; }

    [Column("TaxID")]
    [StringLength(50)]
    public string? TaxId { get; set; }

    [StringLength(200)]
    public string SupplierName { get; set; } = null!;

    [StringLength(200)]
    public string SupplierEmail { get; set; } = null!;

    [StringLength(20)]
    public string SupplierPhoneNumber { get; set; } = null!;

    [StringLength(255)]
    public string SupplierAddress { get; set; } = null!;

    [StringLength(255)]
    public string? ContactPersion { get; set; }

    [StringLength(255)]
    public string? SupplierDescription { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Supplier")]
    public virtual ICollection<ImportStock> ImportStocks { get; set; } = new List<ImportStock>();
}
