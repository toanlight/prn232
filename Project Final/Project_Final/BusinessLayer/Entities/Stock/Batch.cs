using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Partners;

namespace BusinessLayer.Entities.Stock;

public class Batch : BaseEntity
{
    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    public int? SupplierId { get; set; }
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    [Required]
    [StringLength(50)]
    public string LotNumber { get; set; } = default!;

    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal InitialQty { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Active;
    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
    public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
}
