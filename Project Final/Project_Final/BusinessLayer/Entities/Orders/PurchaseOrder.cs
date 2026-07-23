using BusinessLayer.Common;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Orders;

public class PurchaseOrder : BaseEntity, IAuditable
{
    public string PONumber { get; set; } = default!;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly OrderDate { get; set; }
    public DateOnly? ExpectedDate { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "DRAFT";  // DRAFT|SUBMITTED|PARTIALLY_RECEIVED|RECEIVED|CANCELLED
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<POLine> Lines { get; set; } = new List<POLine>();
    public ICollection<GoodsReceiptNote> GRNs { get; set; } = new List<GoodsReceiptNote>();
}

public class POLine : BaseEntity
{
    public int POId { get; set; }
    public PurchaseOrder PO { get; set; } = default!;
    public int ProductId { get; set; }
    public Products.Product Product { get; set; } = default!;
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
}
