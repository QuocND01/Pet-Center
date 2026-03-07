using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Models;

public partial class ImportStockDetail
{
    [Key]
    [Column("ImportStockDetailID")]
    public Guid ImportStockDetailId { get; set; }

    [Column("ImportID")]
    public Guid? ImportId { get; set; }

    [Column("ProductID")]
    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ImportPrice { get; set; }

    public int? StockLeft { get; set; }

    [ForeignKey("ImportId")]
    [InverseProperty("ImportStockDetails")]
    public virtual ImportStock? Import { get; set; }
}
