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

public class GoodsDispatchNote : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string GDNNumber { get; set; } = default!;

    public int? RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public DispatchRequest? Request { get; set; }

    public int CustomerId { get; set; }
    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = default!;

    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    public DateOnly DispatchDate { get; set; }

    [StringLength(255)]
    public string? DeliveryAddress { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(1000)]
    public string? AttachmentUrls { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public bool IsReturn { get; set; }
    public int? ParentGDNId { get; set; }
    [ForeignKey(nameof(ParentGDNId))]
    public GoodsDispatchNote? ParentGDN { get; set; }

    public DateTime? PickedAt { get; set; }
    public int? PickedBy { get; set; }
    [ForeignKey(nameof(PickedBy))]
    public User? PickedByUser { get; set; }

    public DateTime? DeliveredAt { get; set; }
    public int? DeliveredBy { get; set; }
    [ForeignKey(nameof(DeliveredBy))]
    public User? DeliveredByUser { get; set; }

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GDNLine> Lines { get; set; } = new List<GDNLine>();
}

public class GDNLine : BaseEntity
{
    public int GDNId { get; set; }
    [ForeignKey(nameof(GDNId))]
    public GoodsDispatchNote GDN { get; set; } = default!;

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    public int BatchId { get; set; }
    [ForeignKey(nameof(BatchId))]
    public Batch Batch { get; set; } = default!;

    public int BinId { get; set; }
    [ForeignKey(nameof(BinId))]
    public Bin Bin { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal RequestedQty { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? PickedQty { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }
}
