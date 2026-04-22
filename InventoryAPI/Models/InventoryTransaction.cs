using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class InventoryTransaction
{
    public Guid TransactionId { get; set; }

    public Guid InventoryId { get; set; }

    public int QuantityChange { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid ReferenceId { get; set; }

    public string ReferenceType { get; set; } = null!;

    public string? Note { get; set; }

    public virtual Inventory Inventory { get; set; } = null!;
}
