using BusinessLayer.Entities.Orders;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class StockCountRepository : GenericRepository<StockCount>, IStockCountRepository
{
    private readonly StockCountDAO _countDao;

    public StockCountRepository(WmsDbContext context) : base(context)
    {
        _countDao = new StockCountDAO(context);
    }

    public async Task<StockCount?> GetByCountNumberAsync(string countNumber) => await _countDao.GetByCountNumberAsync(countNumber);
    public async Task<StockCount?> GetWithDetailsByIdAsync(int id) => await _countDao.GetWithDetailsByIdAsync(id);
}
