using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Warehouses;

namespace DataAccessLayer.DAO;

public class BinDAO : GenericDAO<Bin>
{
    public BinDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<Bin?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(b => b.Shelf)
                .ThenInclude(s => s.Rack)
                    .ThenInclude(r => r.Zone)
                        .ThenInclude(z => z.Warehouse)
            .FirstOrDefaultAsync(b => b.Code.ToLower() == code.ToLower());
    }

    public async Task<List<Bin>> GetByShelfIdAsync(int shelfId)
    {
        return await _dbSet
            .Where(b => b.ShelfId == shelfId && b.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
