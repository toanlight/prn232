using BusinessLayer.Entities.Identity;

namespace Repositories.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
    Task RevokeTokenAsync(string token);
}
