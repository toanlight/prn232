using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Orders;

public class TransferOrder : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TransferNumber { get; set; } = default!;

    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    public DateOnly TransferDate { get; set; }

    [StringLength(255)]
    public string? Reason { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<TransferOrderLine> Lines { get; set; } = new List<TransferOrderLine>();
}

public class TransferOrderLine : BaseEntity
{
    public int TransferId { get; set; }
    [ForeignKey(nameof(TransferId))]
    public TransferOrder Transfer { get; set; } = default!;

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = default!;

    public int BatchId { get; set; }
    [ForeignKey(nameof(BatchId))]
    public Batch Batch { get; set; } = default!;

    public int FromBinId { get; set; }
    [ForeignKey(nameof(FromBinId))]
    public Bin FromBin { get; set; } = default!;

    public int ToBinId { get; set; }
    [ForeignKey(nameof(ToBinId))]
    public Bin ToBin { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }
}
