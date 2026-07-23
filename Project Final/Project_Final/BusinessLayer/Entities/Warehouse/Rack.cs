using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Warehouses;

public class Rack : BaseEntity
{
    public int ZoneId { get; set; }
    [ForeignKey(nameof(ZoneId))]
    public Zone Zone { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
}
