using BusinessLayer.Enums;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Stock;

public class StockTransaction
{
    public long Id { get; set; }   // bigint vì đây là bảng lớn nhất, tăng nhanh nhất
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public StockTxnType TxnType { get; set; }
    public DocumentType DocumentType { get; set; }
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = default!;
    public decimal Quantity { get; set; }        // luôn dương; chiều +/- xác định bởi TxnType
    public decimal QtyBefore { get; set; }
    public decimal QtyAfter { get; set; }
    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
