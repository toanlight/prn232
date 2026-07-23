using System.ComponentModel.DataAnnotations;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class User : BaseEntity, IAuditable
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = default!;

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = default!;

    [StringLength(100)]
    public string? FullNameEn { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(10)]
    public string PreferredLang { get; set; } = "vi";

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
