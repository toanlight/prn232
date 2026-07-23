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
    public string GDNNumber { get; set; } = default!;
    public int? RequestId { get; set; }
    public DispatchRequest? Request { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly DispatchDate { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrls { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public bool IsReturn { get; set; }
    public int? ParentGDNId { get; set; }
    public GoodsDispatchNote? ParentGDN { get; set; }

    public DateTime? PickedAt { get; set; }
    public int? PickedBy { get; set; }
    public User? PickedByUser { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int? DeliveredBy { get; set; }
    public User? DeliveredByUser { get; set; }

    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GDNLine> Lines { get; set; } = new List<GDNLine>();
}

public class GDNLine : BaseEntity
{
    public int GDNId { get; set; }
    public GoodsDispatchNote GDN { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }               // Batch được chọn theo FEFO
    public Batch Batch { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public decimal RequestedQty { get; set; }
    public decimal? PickedQty { get; set; }         // Số lượng thực tế lấy
    public string? Notes { get; set; }
}
