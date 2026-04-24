using System;
using System.Collections.Generic;

namespace ImportAPI.Models;

public partial class ImportStockDetail
{
    public Guid ImportStockDetailsId { get; set; }

    public Guid ImportId { get; set; }

    public Guid ProductId { get; set; }

    public decimal ImportPrice { get; set; }

    public int Quantity { get; set; }

    public int StockLeft { get; set; }

    public virtual ImportStock Import { get; set; } = null!;

    public virtual ImportProductSnapshot? ImportProductSnapshot { get; set; }
}
