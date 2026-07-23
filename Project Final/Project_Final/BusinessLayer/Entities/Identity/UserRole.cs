using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = default!;
    public DateTime AssignedAt { get; set; }
    public int? AssignedBy { get; set; }
}
