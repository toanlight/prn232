using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Stock;

namespace DataAccessLayer.DAO;

public class BinStockDAO : GenericDAO<BinStock>
{
    public BinStockDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<List<BinStock>> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(bs => bs.Bin)
                .ThenInclude(b => b.Shelf)
                    .ThenInclude(s => s.Rack)
                        .ThenInclude(r => r.Zone)
                            .ThenInclude(z => z.Warehouse)
            .Include(bs => bs.Batch)
            .Where(bs => bs.ProductId == productId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<BinStock>> GetByBinIdAsync(int binId)
    {
        return await _dbSet
            .Include(bs => bs.Product)
            .Include(bs => bs.Batch)
            .Where(bs => bs.BinId == binId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<BinStock?> GetSpecificStockAsync(int binId, int productId, int batchId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(bs => bs.BinId == binId && 
                                       bs.ProductId == productId && 
                                       bs.BatchId == batchId);
    }
}
