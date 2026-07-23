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
    public string GRNNumber { get; set; } = default!;
    public int? POId { get; set; }
    public PurchaseOrder? PO { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly ReceiptDate { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrls { get; set; }   // JSON array

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public bool IsReturn { get; set; }
    public int? ParentGRNId { get; set; }
    public GoodsReceiptNote? ParentGRN { get; set; }

    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<GRNLine> Lines { get; set; } = new List<GRNLine>();
}

public class GRNLine : BaseEntity
{
    public int GRNId { get; set; }
    public GoodsReceiptNote GRN { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int? BatchId { get; set; }             // Null cho đến khi Batch được tạo/khớp
    public Batch? Batch { get; set; }
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public decimal Quantity { get; set; }

    // Thông tin lô — dùng để tạo Batch mới nếu chưa tồn tại
    public string? LotNumber { get; set; }
    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
}
