using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Orders;

public class StockCount : BaseEntity
{
    public string CountNumber { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public CountType CountType { get; set; }
    public int? ZoneId { get; set; }               // Null = toàn kho
    public Zone? Zone { get; set; }
    public int? RackId { get; set; }
    public Rack? Rack { get; set; }
    public DateOnly CountDate { get; set; }
    public int PlannedBy { get; set; }
    public User PlannedByUser { get; set; } = default!;
    public string? Notes { get; set; }
    // PLANNED|IN_PROGRESS|COMPLETED|ADJUSTMENT_PENDING|CLOSED
    public string Status { get; set; } = "PLANNED";
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<StockCountLine> Lines { get; set; } = new List<StockCountLine>();
    public ICollection<StockAdjustment> Adjustments { get; set; } = new List<StockAdjustment>();
}

public class StockCountLine : BaseEntity
{
    public int CountId { get; set; }
    public StockCount Count { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public decimal SystemQty { get; set; }          // Snapshot khi bắt đầu kiểm
    public decimal? ActualQty { get; set; }          // WH_STAFF nhập
    
    [NotMapped]
    public decimal Variance => (ActualQty ?? 0) - SystemQty;
    
    public int? CountedBy { get; set; }
    public User? CountedByUser { get; set; }
    public DateTime? CountedAt { get; set; }
    public string? Notes { get; set; }
}
