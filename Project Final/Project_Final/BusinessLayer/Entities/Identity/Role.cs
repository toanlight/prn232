using System.ComponentModel.DataAnnotations;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class Role : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RoleCode { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string RoleNameEn { get; set; } = default!;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
