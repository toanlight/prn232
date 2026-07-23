using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Orders;

public class PurchaseOrder : BaseEntity, IAuditable
{
    [Required]
    [StringLength(50)]
    public string PONumber { get; set; } = default!;

    public int SupplierId { get; set; }
    [ForeignKey(nameof(SupplierId))]
    public Supplier Supplier { get; set; } = default!;

    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    public DateOnly OrderDate { get; set; }
    public DateOnly? ExpectedDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "DRAFT";

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<POLine> Lines { get; set; } = new List<POLine>();
    public ICollection<GoodsReceiptNote> GRNs { get; set; } = new List<GoodsReceiptNote>();
}

public class POLine : BaseEntity
{
    public int POId { get; set; }
    [ForeignKey(nameof(POId))]
    public PurchaseOrder PO { get; set; } = default!;

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Products.Product Product { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal OrderedQty { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ReceivedQty { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? UnitPrice { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }
}
