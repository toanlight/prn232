using BusinessLayer.Entities.Stock;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class StockTransactionRepository : GenericRepository<StockTransaction>, IStockTransactionRepository
{
    private readonly StockTransactionDAO _txnDao;

    public StockTransactionRepository(WmsDbContext context) : base(context)
    {
        _txnDao = new StockTransactionDAO(context);
    }

    public async Task<(List<StockTransaction> Items, int TotalCount)> GetHistoryAsync(int? productId, int? binId, string? documentNumber, int pageIndex = 1, int pageSize = 20)
        => await _txnDao.GetHistoryAsync(productId, binId, documentNumber, pageIndex, pageSize);
}
