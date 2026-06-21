using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class Inventory
{
    public Guid InventoryId { get; set; }

    public Guid ProductId { get; set; }

    public int QuantityAvailable { get; set; }

    public int QuantityReserved { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual Product Product { get; set; } = null!;
}
