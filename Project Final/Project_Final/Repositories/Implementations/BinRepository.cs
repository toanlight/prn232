using BusinessLayer.Entities.Warehouses;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class BinRepository : GenericRepository<Bin>, IBinRepository
{
    private readonly BinDAO _binDao;

    public BinRepository(WmsDbContext context) : base(context)
    {
        _binDao = new BinDAO(context);
    }

    public async Task<List<Bin>> GetByShelfIdAsync(int shelfId) => await _binDao.GetByShelfIdAsync(shelfId);
}
