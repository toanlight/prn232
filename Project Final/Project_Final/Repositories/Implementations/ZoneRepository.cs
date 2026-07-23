using BusinessLayer.Entities.Warehouses;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class ZoneRepository : GenericRepository<Zone>, IZoneRepository
{
    private readonly ZoneDAO _zoneDao;

    public ZoneRepository(WmsDbContext context) : base(context)
    {
        _zoneDao = new ZoneDAO(context);
    }

    public async Task<List<Zone>> GetByWarehouseIdAsync(int warehouseId) => await _zoneDao.GetByWarehouseIdAsync(warehouseId);
}
