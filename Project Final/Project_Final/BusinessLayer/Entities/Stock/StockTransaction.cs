using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Stock;

public class StockTransaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    public int BatchId { get; set; }
    [ForeignKey(nameof(BatchId))]
    public Batch Batch { get; set; } = default!;

    public int BinId { get; set; }
    [ForeignKey(nameof(BinId))]
    public Bin Bin { get; set; } = default!;

    public StockTxnType TxnType { get; set; }
    public DocumentType DocumentType { get; set; }
    public int DocumentId { get; set; }

    [Required]
    [StringLength(50)]
    public string DocumentNumber { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal QtyBefore { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal QtyAfter { get; set; }

    [StringLength(255)]
    public string? Remarks { get; set; }

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
