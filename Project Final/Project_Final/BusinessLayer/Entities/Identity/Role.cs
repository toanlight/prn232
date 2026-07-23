using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class Role : BaseEntity
{
    public string RoleCode { get; set; } = default!;   // SYS_ADMIN, DIRECTOR, WH_MANAGER...
    public string RoleName { get; set; } = default!;    // Tiếng Việt
    public string RoleNameEn { get; set; } = default!;  // Tiếng Anh
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
