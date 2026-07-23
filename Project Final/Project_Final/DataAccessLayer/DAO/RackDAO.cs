using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Warehouses;

namespace DataAccessLayer.DAO;

public class RackDAO : GenericDAO<Rack>
{
    public RackDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<List<Rack>> GetByZoneIdAsync(int zoneId)
    {
        return await _dbSet
            .Include(r => r.Shelves)
            .Where(r => r.ZoneId == zoneId && r.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
