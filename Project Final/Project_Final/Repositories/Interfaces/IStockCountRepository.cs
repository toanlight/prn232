using BusinessLayer.Entities.Orders;

namespace Repositories.Interfaces;

public interface IStockCountRepository : IGenericRepository<StockCount>
{
    Task<StockCount?> GetByCountNumberAsync(string countNumber);
    Task<StockCount?> GetWithDetailsByIdAsync(int id);
}
