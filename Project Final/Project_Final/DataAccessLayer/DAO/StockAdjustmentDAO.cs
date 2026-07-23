using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Orders;

namespace DataAccessLayer.DAO;

public class StockAdjustmentDAO : GenericDAO<StockAdjustment>
{
    public StockAdjustmentDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<StockAdjustment?> GetByAdjNumberAsync(string adjNumber)
    {
        return await _dbSet
            .Include(sa => sa.Warehouse)
            .Include(sa => sa.CreatedByUser)
            .Include(sa => sa.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(sa => sa.AdjNumber.ToLower() == adjNumber.ToLower());
    }

    public async Task<StockAdjustment?> GetWithDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(sa => sa.Warehouse)
            .Include(sa => sa.CreatedByUser)
            .Include(sa => sa.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(sa => sa.Id == id);
    }
}
