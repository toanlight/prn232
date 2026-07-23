using BusinessLayer.Entities.Stock;

namespace Repositories.Interfaces;

public interface IBinStockRepository : IGenericRepository<BinStock>
{
    Task<List<BinStock>> GetByProductIdAsync(int productId);
    Task<List<BinStock>> GetByBinIdAsync(int binId);
    Task<BinStock?> GetSpecificStockAsync(int binId, int productId, int batchId);
}
