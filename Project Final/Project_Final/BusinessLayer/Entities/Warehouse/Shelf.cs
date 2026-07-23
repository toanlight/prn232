using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Warehouses;

public class Shelf : BaseEntity
{
    public int RackId { get; set; }
    [ForeignKey(nameof(RackId))]
    public Rack Rack { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Bin> Bins { get; set; } = new List<Bin>();
}
