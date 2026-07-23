using BusinessLayer.Entities.Stock;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class BatchRepository : GenericRepository<Batch>, IBatchRepository
{
    private readonly BatchDAO _batchDao;

    public BatchRepository(WmsDbContext context) : base(context)
    {
        _batchDao = new BatchDAO(context);
    }

    public async Task<Batch?> GetByLotNumberAsync(string lotNumber) => await _batchDao.GetByLotNumberAsync(lotNumber);
    public async Task<List<Batch>> GetByProductIdAsync(int productId) => await _batchDao.GetByProductIdAsync(productId);
    public async Task<List<Batch>> GetExpiringBatchesAsync(int warningDays = 30) => await _batchDao.GetExpiringBatchesAsync(warningDays);
}
