using System.ComponentModel.DataAnnotations;
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
    [Required]
    [StringLength(50)]
    public string CountNumber { get; set; } = default!;

    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    public CountType CountType { get; set; }

    public int? ZoneId { get; set; }
    [ForeignKey(nameof(ZoneId))]
    public Zone? Zone { get; set; }

    public int? RackId { get; set; }
    [ForeignKey(nameof(RackId))]
    public Rack? Rack { get; set; }

    public DateOnly CountDate { get; set; }

    public int PlannedBy { get; set; }
    [ForeignKey(nameof(PlannedBy))]
    public User PlannedByUser { get; set; } = default!;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    [StringLength(30)]
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
    [ForeignKey(nameof(CountId))]
    public StockCount Count { get; set; } = default!;

    public int BinId { get; set; }
    [ForeignKey(nameof(BinId))]
    public Bin Bin { get; set; } = default!;

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    public int BatchId { get; set; }
    [ForeignKey(nameof(BatchId))]
    public Batch Batch { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal SystemQty { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? ActualQty { get; set; }

    [NotMapped]
    public decimal Variance => (ActualQty ?? 0) - SystemQty;

    public int? CountedBy { get; set; }
    [ForeignKey(nameof(CountedBy))]
    public User? CountedByUser { get; set; }

    public DateTime? CountedAt { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }
}
