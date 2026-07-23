using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Warehouses;

public class Warehouse : BaseEntity, IAuditable
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? Address { get; set; }
    public int? ManagerUserId { get; set; }
    public User? ManagerUser { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}
