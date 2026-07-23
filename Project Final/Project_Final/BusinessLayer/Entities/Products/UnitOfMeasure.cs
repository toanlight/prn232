using System.ComponentModel.DataAnnotations;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Products;

public class UnitOfMeasure : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = default!;

    [StringLength(50)]
    public string? NameEn { get; set; }

    public bool IsActive { get; set; } = true;
}
