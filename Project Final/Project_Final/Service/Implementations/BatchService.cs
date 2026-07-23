using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class BatchService : IBatchService
{
    private readonly IBatchRepository _batchRepo;

    public BatchService(IBatchRepository batchRepo)
        => _batchRepo = batchRepo;

    public async Task<List<Batch>> GetBatchesAsync(int? productId, int? supplierId, BatchStatus? status, int pageIndex = 1, int pageSize = 20)
    {
        var query = _batchRepo.GetQueryable();

        if (productId.HasValue)
            query = query.Where(b => b.ProductId == productId.Value);
        if (supplierId.HasValue)
            query = query.Where(b => b.SupplierId == supplierId.Value);
        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        return await query
            .OrderByDescending(b => b.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Batch> GetByIdAsync(int id)
        => await _batchRepo.GetByIdAsync(id)
           ?? throw new KeyNotFoundException($"Không tìm thấy lô hàng ID={id}.");

    public async Task<List<Batch>> GetExpiringBatchesAsync(int warningDays = 30)
        => await _batchRepo.GetExpiringBatchesAsync(warningDays);

    public async Task<List<Batch>> GetExpiredBatchesAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _batchRepo.GetQueryable()
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value < today && b.Status == BatchStatus.Active)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task UpdateBatchStatusAsync(int id, BatchStatus status)
    {
        var batch = await _batchRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy lô hàng ID={id}.");
        batch.Status = status;
        _batchRepo.Update(batch);
        await _batchRepo.SaveChangesAsync();
    }
}
