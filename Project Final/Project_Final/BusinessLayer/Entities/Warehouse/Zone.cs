using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Warehouses;

public class Zone : BaseEntity
{
    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(100)]
    public string? NameEn { get; set; }

    [StringLength(50)]
    public string? ZoneType { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Rack> Racks { get; set; } = new List<Rack>();
}
