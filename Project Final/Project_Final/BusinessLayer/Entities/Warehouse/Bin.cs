using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Stock;

namespace BusinessLayer.Entities.Warehouses;

public class Bin : BaseEntity
{
    public int ShelfId { get; set; }
    [ForeignKey(nameof(ShelfId))]
    public Shelf Shelf { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [Column(TypeName = "decimal(18,4)")]
    public decimal? MaxCapacity { get; set; }

    [StringLength(20)]
    public string? CapacityUnit { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
}
