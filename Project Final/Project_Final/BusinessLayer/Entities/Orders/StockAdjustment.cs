using System.ComponentModel.DataAnnotations;
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
    [Required]
    [StringLength(50)]
    public string AdjNumber { get; set; } = default!;

    public int? CountId { get; set; }
    [ForeignKey(nameof(CountId))]
    public StockCount? Count { get; set; }

    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    [Required]
    [StringLength(255)]
    public string Reason { get; set; } = default!;

    [StringLength(500)]
    public string? Notes { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<StockAdjustmentLine> Lines { get; set; } = new List<StockAdjustmentLine>();
}

public class StockAdjustmentLine : BaseEntity
{
    public int AdjustmentId { get; set; }
    [ForeignKey(nameof(AdjustmentId))]
    public StockAdjustment Adjustment { get; set; } = default!;

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
    public decimal BeforeQty { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal AfterQty { get; set; }

    [NotMapped]
    public decimal DeltaQty => AfterQty - BeforeQty;

    [StringLength(255)]
    public string? Notes { get; set; }
}
