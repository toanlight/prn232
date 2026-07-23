using BusinessLayer.Entities.Identity;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly RefreshTokenDAO _tokenDao;

    public RefreshTokenRepository(WmsDbContext context) : base(context)
    {
        _tokenDao = new RefreshTokenDAO(context);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token) => await _tokenDao.GetByTokenAsync(token);
    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId) => await _tokenDao.GetActiveTokensByUserIdAsync(userId);
    public async Task RevokeTokenAsync(string token) => await _tokenDao.RevokeTokenAsync(token);
}
