using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Warehouses;

public class Warehouse : BaseEntity, IAuditable
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = default!;

    [StringLength(150)]
    public string? NameEn { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public int? ManagerUserId { get; set; }
    [ForeignKey(nameof(ManagerUserId))]
    public User? ManagerUser { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}
