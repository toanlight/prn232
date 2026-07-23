using BusinessLayer.Common;

namespace BusinessLayer.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}
