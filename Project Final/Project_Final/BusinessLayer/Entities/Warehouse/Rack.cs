using BusinessLayer.Common;

namespace BusinessLayer.Entities.Warehouses;

public class Rack : BaseEntity
{
    public int ZoneId { get; set; }
    public Zone Zone { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
}
