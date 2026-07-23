using BusinessLayer.Entities.Warehouses;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class RackRepository : GenericRepository<Rack>, IRackRepository
{
    private readonly RackDAO _rackDao;

    public RackRepository(WmsDbContext context) : base(context)
    {
        _rackDao = new RackDAO(context);
    }

    public async Task<List<Rack>> GetByZoneIdAsync(int zoneId) => await _rackDao.GetByZoneIdAsync(zoneId);
}
