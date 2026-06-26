using System;
using System.Collections.Generic;

namespace PetCenterAPI.Models;

public partial class InventoryTransaction
{
    public Guid TransactionId { get; set; }
    public Guid InventoryId { get; set; }

    
    public int QuantityChange { get; set; }

    // 🆕 snapshot trước/sau để reconstruct lịch sử
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }

    // new
    public TransactionType TransactionType { get; set; }

    public DateTime CreatedAt { get; set; }

    // new
    public Guid CreatedBy { get; set; }

    // đổi sang nullable — manual adjust không có reference
    public Guid? ReferenceId { get; set; }

    // ✅ đổi sang enum cho type-safe
    public ReferenceType? ReferenceType { get; set; }

    // 🆕 FK sang lô hàng — để FEFO trừ đúng StockLeft
    public Guid? ImportStockDetailId { get; set; }

    public string? Note { get; set; }

    // Navigation
    public virtual Inventory Inventory { get; set; } = null!;
    public virtual ImportStockDetail? ImportStockDetail { get; set; }
}

public enum TransactionType
{
    StockIn,      // nhập kho từ ImportStock
    StockOut,     // xuất kho theo Order
    Reserve,      // giữ hàng khi tạo đơn
    Release,      // hủy giữ khi cancel đơn
    Adjust,       // điều chỉnh thủ công
    Damage        // ghi nhận hàng hỏng
}

public enum ReferenceType
{
    Order,
    ImportStock,
    Manual
}
