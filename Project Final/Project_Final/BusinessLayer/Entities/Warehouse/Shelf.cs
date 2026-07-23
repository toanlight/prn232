using BusinessLayer.Common;

namespace BusinessLayer.Entities.Warehouses;

public class Shelf : BaseEntity
{
    public int RackId { get; set; }
    public Rack Rack { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Bin> Bins { get; set; } = new List<Bin>();
}
