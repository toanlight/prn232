using BusinessLayer.Common;

namespace BusinessLayer.Entities.Warehouses;

public class Zone : BaseEntity
{
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? ZoneType { get; set; }   // COLD, DRY, HAZMAT, GENERAL
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Rack> Racks { get; set; } = new List<Rack>();
}
