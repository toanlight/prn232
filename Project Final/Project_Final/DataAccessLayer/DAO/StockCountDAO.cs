using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Orders;

namespace DataAccessLayer.DAO;

public class StockCountDAO : GenericDAO<StockCount>
{
    public StockCountDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<StockCount?> GetByCountNumberAsync(string countNumber)
    {
        return await _dbSet
            .Include(sc => sc.Warehouse)
            .Include(sc => sc.PlannedByUser)
            .Include(sc => sc.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(sc => sc.CountNumber.ToLower() == countNumber.ToLower());
    }

    public async Task<StockCount?> GetWithDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(sc => sc.Warehouse)
            .Include(sc => sc.PlannedByUser)
            .Include(sc => sc.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }
}
