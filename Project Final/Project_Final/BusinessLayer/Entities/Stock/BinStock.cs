using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Products;

namespace BusinessLayer.Entities.Stock;

public class BinStock : BaseEntity
{
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public int ProductId { get; set; }          // Denormalized cho query performance
    public Product Product { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal ReservedQty { get; set; }     // Giữ chỗ bởi GDN đang PENDING/APPROVED
    public DateTime UpdatedAt { get; set; }

    [NotMapped]
    public decimal AvailableQty => Quantity - ReservedQty;
}
