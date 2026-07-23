using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Warehouses;

namespace DataAccessLayer.DAO;

public class WarehouseDAO : GenericDAO<Warehouse>
{
    public WarehouseDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<Warehouse?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(w => w.ManagerUser)
            .Include(w => w.Zones)
            .FirstOrDefaultAsync(w => w.Code.ToLower() == code.ToLower());
    }

    public async Task<Warehouse?> GetWithHierarchyByIdAsync(int id)
    {
        return await _dbSet
            .Include(w => w.ManagerUser)
            .Include(w => w.Zones)
                .ThenInclude(z => z.Racks)
                    .ThenInclude(r => r.Shelves)
                        .ThenInclude(s => s.Bins)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<List<Warehouse>> GetActiveWarehousesAsync()
    {
        return await _dbSet
            .Include(w => w.ManagerUser)
            .Where(w => w.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
