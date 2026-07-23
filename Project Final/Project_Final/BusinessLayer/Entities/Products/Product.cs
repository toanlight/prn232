using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Products;

public class Product : BaseEntity, IAuditable
{
    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = default!;

    [StringLength(50)]
    public string? Barcode { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = default!;

    [StringLength(150)]
    public string? NameEn { get; set; }

    public int CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public ProductCategory Category { get; set; } = default!;

    public int UomId { get; set; }
    [ForeignKey(nameof(UomId))]
    public UnitOfMeasure Uom { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal MinStock { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ReorderPoint { get; set; }

    public bool IsBatchTracked { get; set; } = true;
    public bool IsExpiryTracked { get; set; }
    public int ExpiryWarningDays { get; set; } = 30;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
}
