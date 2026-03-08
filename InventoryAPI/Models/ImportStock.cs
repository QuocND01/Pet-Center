using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Models;

public partial class ImportStock
{
    [Key]
    [Column("ImportID")]
    public Guid ImportId { get; set; }

    [Column("SupplierID")]
    public Guid SupplierId { get; set; }

    [Column("StaffID")]
    public Guid StaffId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ImportDate { get; set; }

    public ImportStatus Status { get; set; }

    public enum ImportStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2
    }

    [InverseProperty("Import")]
    public virtual ICollection<ImportStockDetail> ImportStockDetails { get; set; } = new List<ImportStockDetail>();

    [ForeignKey("SupplierId")]
    [InverseProperty("ImportStocks")]
    public virtual Supplier Supplier { get; set; } = null!;
}
