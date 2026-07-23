using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Stock;

namespace DataAccessLayer.DAO;

public class BatchDAO : GenericDAO<Batch>
{
    public BatchDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<Batch?> GetByLotNumberAsync(string lotNumber)
    {
        return await _dbSet
            .Include(b => b.Product)
            .Include(b => b.Supplier)
            .FirstOrDefaultAsync(b => b.LotNumber.ToLower() == lotNumber.ToLower());
    }

    public async Task<List<Batch>> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(b => b.Supplier)
            .Where(b => b.ProductId == productId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Batch>> GetExpiringBatchesAsync(int warningDays = 30)
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(warningDays));
        return await _dbSet
            .Include(b => b.Product)
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value <= targetDate)
            .AsNoTracking()
            .ToListAsync();
    }
}
