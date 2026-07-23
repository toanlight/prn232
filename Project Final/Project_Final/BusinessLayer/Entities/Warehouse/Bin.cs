using BusinessLayer.Common;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Warehouses;

public class Bin : BaseEntity
{
    public int ShelfId { get; set; }
    public Shelf Shelf { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal? MaxCapacity { get; set; }
    public string? CapacityUnit { get; set; }   // KG, CBM, UNIT
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
}
