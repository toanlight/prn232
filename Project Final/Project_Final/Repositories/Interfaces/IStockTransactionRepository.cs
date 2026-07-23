using BusinessLayer.Entities.Stock;

namespace Repositories.Interfaces;

public interface IStockTransactionRepository : IGenericRepository<StockTransaction>
{
    Task<(List<StockTransaction> Items, int TotalCount)> GetHistoryAsync(int? productId, int? binId, string? documentNumber, int pageIndex = 1, int pageSize = 20);
}
