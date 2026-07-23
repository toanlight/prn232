using BusinessLayer.Entities.Orders;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class StockAdjustmentRepository : GenericRepository<StockAdjustment>, IStockAdjustmentRepository
{
    private readonly StockAdjustmentDAO _adjDao;

    public StockAdjustmentRepository(WmsDbContext context) : base(context)
    {
        _adjDao = new StockAdjustmentDAO(context);
    }

    public async Task<StockAdjustment?> GetByAdjNumberAsync(string adjNumber) => await _adjDao.GetByAdjNumberAsync(adjNumber);
    public async Task<StockAdjustment?> GetWithDetailsByIdAsync(int id) => await _adjDao.GetWithDetailsByIdAsync(id);
}
