using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;

namespace Service.Interfaces;

public interface IBatchService
{
    Task<List<Batch>> GetBatchesAsync(int? productId, int? supplierId, BatchStatus? status, int pageIndex = 1, int pageSize = 20);
    Task<Batch> GetByIdAsync(int id);
    Task<List<Batch>> GetExpiringBatchesAsync(int warningDays = 30);
    Task<List<Batch>> GetExpiredBatchesAsync();
    Task UpdateBatchStatusAsync(int id, BatchStatus status);
}
