using BusinessLayer.Common;

namespace BusinessLayer.Entities.Products;

public class UnitOfMeasure : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public bool IsActive { get; set; } = true;
}
