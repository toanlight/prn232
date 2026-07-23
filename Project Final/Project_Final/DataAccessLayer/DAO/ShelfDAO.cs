using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Warehouses;

namespace DataAccessLayer.DAO;

public class ShelfDAO : GenericDAO<Shelf>
{
    public ShelfDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<List<Shelf>> GetByRackIdAsync(int rackId)
    {
        return await _dbSet
            .Include(s => s.Bins)
            .Where(s => s.RackId == rackId && s.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
