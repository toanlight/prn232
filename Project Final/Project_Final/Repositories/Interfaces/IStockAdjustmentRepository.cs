using BusinessLayer.Entities.Orders;

namespace Repositories.Interfaces;

public interface IStockAdjustmentRepository : IGenericRepository<StockAdjustment>
{
    Task<StockAdjustment?> GetByAdjNumberAsync(string adjNumber);
    Task<StockAdjustment?> GetWithDetailsByIdAsync(int id);
}
