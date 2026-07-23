using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class User : BaseEntity, IAuditable
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? FullNameEn { get; set; }
    public string? Phone { get; set; }
    public string PreferredLang { get; set; } = "vi";   // vi | en
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
