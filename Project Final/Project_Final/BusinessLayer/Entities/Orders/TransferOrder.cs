using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Orders;

public class TransferOrder : BaseEntity
{
    public string TransferNumber { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly TransferDate { get; set; }
    public string? Reason { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<TransferOrderLine> Lines { get; set; } = new List<TransferOrderLine>();
}

public class TransferOrderLine : BaseEntity
{
    public int TransferId { get; set; }
    public TransferOrder Transfer { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public int FromBinId { get; set; }
    public Bin FromBin { get; set; } = default!;
    public int ToBinId { get; set; }
    public Bin ToBin { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}
