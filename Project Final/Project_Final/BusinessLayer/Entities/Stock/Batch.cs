using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Partners;

namespace BusinessLayer.Entities.Stock;

public class Batch : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string LotNumber { get; set; } = default!;
    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal InitialQty { get; set; }
    public string? Notes { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Active;
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;

    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
    public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
}
