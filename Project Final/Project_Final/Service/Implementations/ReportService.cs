using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class ReportService : IReportService
{
    private readonly IStockService _stockService;
    private readonly IStockTransactionRepository _txnRepo;
    private readonly IBatchRepository _batchRepo;

    public ReportService(
        IStockService stockService,
        IStockTransactionRepository txnRepo,
        IBatchRepository batchRepo)
    {
        _stockService = stockService;
        _txnRepo = txnRepo;
        _batchRepo = batchRepo;
    }

    public async Task<List<StockSummaryDto>> GetCurrentStockReportAsync(int? warehouseId, int? categoryId, bool? belowMinStock)
    {
        return await _stockService.GetCurrentStockAsync(warehouseId, categoryId, belowMinStock);
    }

    public async Task<List<StockTransaction>> GetInventoryMovementReportAsync(
        DateTime? from, DateTime? to, int? warehouseId, int? productId, string? txnType)
    {
        var query = _txnRepo.GetQueryable()
            .Include(t => t.Product)
            .Include(t => t.Batch)
            .Include(t => t.Bin)
            .AsNoTracking();

        if (from.HasValue)
            query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(t => t.CreatedAt <= to.Value);
        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);
        if (warehouseId.HasValue)
            query = query.Where(t => t.Bin.Shelf.Rack.Zone.WarehouseId == warehouseId.Value);
        if (Enum.TryParse<StockTxnType>(txnType, true, out var parsedTxnType))
            query = query.Where(t => t.TxnType == parsedTxnType);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<List<Batch>> GetExpiryReportAsync(int? warehouseId, int warningDays = 30)
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(warningDays));

        var query = _batchRepo.GetQueryable()
            .Include(b => b.Product)
            .Include(b => b.Supplier)
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value <= targetDate && b.Status == BatchStatus.Active)
            .AsNoTracking();

        return await query.OrderBy(b => b.ExpiryDate).ToListAsync();
    }

    public async Task<byte[]> ExportReportAsync(string reportType, string format, Dictionary<string, string> parameters)
    {
        // TODO: Kết nối thư viện ClosedXML (Excel) / QuestPDF trong Phase 4
        // Trả về byte array trống để tránh break build
        return await Task.FromResult(Array.Empty<byte>());
    }
}
