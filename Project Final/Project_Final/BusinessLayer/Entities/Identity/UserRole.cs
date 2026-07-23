using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    public int RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = default!;

    public DateTime AssignedAt { get; set; }
    public int? AssignedBy { get; set; }
}
