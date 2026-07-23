using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    [Required]
    [StringLength(500)]
    public string Token { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}
