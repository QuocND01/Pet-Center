using System;
using System.Collections.Generic;

namespace Pet_Center.Models;

public partial class ImportStockDetail
{
    public Guid ImportStockDetailId { get; set; }

    public Guid? ImportId { get; set; }

    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public int? StockLeft { get; set; }

    public virtual ImportStock? Import { get; set; }

    public virtual Product? Product { get; set; }
}
