using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Warehouses;

namespace DataAccessLayer.DAO;

public class ZoneDAO : GenericDAO<Zone>
{
    public ZoneDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<List<Zone>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _dbSet
            .Include(z => z.Racks)
            .Where(z => z.WarehouseId == warehouseId && z.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
