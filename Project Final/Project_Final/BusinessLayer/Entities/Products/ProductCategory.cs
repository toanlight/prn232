using BusinessLayer.Common;

namespace BusinessLayer.Entities.Products;

public class ProductCategory : BaseEntity
{
    public int? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
