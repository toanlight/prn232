using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Stock;

namespace DataAccessLayer.DAO;

public class StockTransactionDAO : GenericDAO<StockTransaction>
{
    public StockTransactionDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<(List<StockTransaction> Items, int TotalCount)> GetHistoryAsync(
        int? productId,
        int? binId,
        string? documentNumber,
        int pageIndex = 1,
        int pageSize = 20)
    {
        var query = _dbSet
            .Include(st => st.Product)
            .Include(st => st.Batch)
            .Include(st => st.Bin)
            .Include(st => st.CreatedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(st => st.ProductId == productId.Value);
        }

        if (binId.HasValue)
        {
            query = query.Where(st => st.BinId == binId.Value);
        }

        if (!string.IsNullOrWhiteSpace(documentNumber))
        {
            query = query.Where(st => st.DocumentNumber.ToLower().Contains(documentNumber.Trim().ToLower()));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(st => st.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
