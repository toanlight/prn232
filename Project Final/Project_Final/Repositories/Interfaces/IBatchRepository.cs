using BusinessLayer.Entities.Stock;

namespace Repositories.Interfaces;

public interface IBatchRepository : IGenericRepository<Batch>
{
    Task<Batch?> GetByLotNumberAsync(string lotNumber);
    Task<List<Batch>> GetByProductIdAsync(int productId);
    Task<List<Batch>> GetExpiringBatchesAsync(int warningDays = 30);
}
