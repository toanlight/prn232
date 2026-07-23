using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Products;

public class Product : BaseEntity, IAuditable
{
    public string SKU { get; set; } = default!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; } = default!;
    public int UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Stock control
    public decimal MinStock { get; set; }
    public decimal ReorderPoint { get; set; }

    // Batch config
    public bool IsBatchTracked { get; set; } = true;
    public bool IsExpiryTracked { get; set; }
    public int ExpiryWarningDays { get; set; } = 30;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
}
