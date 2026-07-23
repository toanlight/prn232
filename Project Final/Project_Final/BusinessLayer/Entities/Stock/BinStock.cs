using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Products;

namespace BusinessLayer.Entities.Stock;

public class BinStock : BaseEntity
{
    public int BinId { get; set; }
    [ForeignKey(nameof(BinId))]
    public Bin Bin { get; set; } = default!;

    public int BatchId { get; set; }
    [ForeignKey(nameof(BatchId))]
    public Batch Batch { get; set; } = default!;

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ReservedQty { get; set; }

    public DateTime UpdatedAt { get; set; }

    [NotMapped]
    public decimal AvailableQty => Quantity - ReservedQty;
}
