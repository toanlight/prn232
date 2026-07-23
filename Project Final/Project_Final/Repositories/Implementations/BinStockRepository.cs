using BusinessLayer.Entities.Stock;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class BinStockRepository : GenericRepository<BinStock>, IBinStockRepository
{
    private readonly BinStockDAO _binStockDao;

    public BinStockRepository(WmsDbContext context) : base(context)
    {
        _binStockDao = new BinStockDAO(context);
    }

    public async Task<List<BinStock>> GetByProductIdAsync(int productId) => await _binStockDao.GetByProductIdAsync(productId);
    public async Task<List<BinStock>> GetByBinIdAsync(int binId) => await _binStockDao.GetByBinIdAsync(binId);
    public async Task<BinStock?> GetSpecificStockAsync(int binId, int productId, int batchId) 
        => await _binStockDao.GetSpecificStockAsync(binId, productId, batchId);
}
