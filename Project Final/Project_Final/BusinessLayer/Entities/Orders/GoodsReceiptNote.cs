using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Orders;

public class GoodsReceiptNote : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string GRNNumber { get; set; } = default!;

    public int? POId { get; set; }
    [ForeignKey(nameof(POId))]
    public PurchaseOrder? PO { get; set; }

    public int SupplierId { get; set; }
    [ForeignKey(nameof(SupplierId))]
    public Supplier Supplier { get; set; } = default!;

    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    public DateOnly ReceiptDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(1000)]
    public string? AttachmentUrls { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public bool IsReturn { get; set; }
    public int? ParentGRNId { get; set; }
    [ForeignKey(nameof(ParentGRNId))]
    public GoodsReceiptNote? ParentGRN { get; set; }

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<GRNLine> Lines { get; set; } = new List<GRNLine>();
}

public class GRNLine : BaseEntity
{
    public int GRNId { get; set; }
    [ForeignKey(nameof(GRNId))]
    public GoodsReceiptNote GRN { get; set; } = default!;

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    public int? BatchId { get; set; }
    [ForeignKey(nameof(BatchId))]
    public Batch? Batch { get; set; }

    public int BinId { get; set; }
    [ForeignKey(nameof(BinId))]
    public Bin Bin { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [StringLength(50)]
    public string? LotNumber { get; set; }

    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? UnitPrice { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }
}
