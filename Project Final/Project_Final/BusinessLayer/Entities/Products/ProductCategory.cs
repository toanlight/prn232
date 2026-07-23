using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Products;

public class ProductCategory : BaseEntity
{
    public int? ParentId { get; set; }
    [ForeignKey(nameof(ParentId))]
    public ProductCategory? Parent { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(100)]
    public string? NameEn { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
