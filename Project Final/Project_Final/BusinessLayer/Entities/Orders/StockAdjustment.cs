using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Orders;

public class StockAdjustment : BaseEntity
{
    public string AdjNumber { get; set; } = default!;
    public int? CountId { get; set; }
    public StockCount? Count { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<StockAdjustmentLine> Lines { get; set; } = new List<StockAdjustmentLine>();
}

public class StockAdjustmentLine : BaseEntity
{
    public int AdjustmentId { get; set; }
    public StockAdjustment Adjustment { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public decimal BeforeQty { get; set; }
    public decimal AfterQty { get; set; }
    
    [NotMapped]
    public decimal DeltaQty => AfterQty - BeforeQty;
    
    public string? Notes { get; set; }
}
