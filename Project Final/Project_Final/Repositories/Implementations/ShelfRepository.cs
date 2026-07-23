using BusinessLayer.Entities.Warehouses;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class ShelfRepository : GenericRepository<Shelf>, IShelfRepository
{
    private readonly ShelfDAO _shelfDao;

    public ShelfRepository(WmsDbContext context) : base(context)
    {
        _shelfDao = new ShelfDAO(context);
    }

    public async Task<List<Shelf>> GetByRackIdAsync(int rackId) => await _shelfDao.GetByRackIdAsync(rackId);
}
