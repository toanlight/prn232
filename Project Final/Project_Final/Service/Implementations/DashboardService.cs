using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IProductRepository _productRepo;
    private readonly IStockTransactionRepository _txnRepo;
    private readonly IBatchRepository _batchRepo;
    private readonly IApprovalRepository _approvalRepo;

    public DashboardService(
        IProductRepository productRepo,
        IStockTransactionRepository txnRepo,
        IBatchRepository batchRepo,
        IApprovalRepository approvalRepo)
    {
        _productRepo = productRepo;
        _txnRepo = txnRepo;
        _batchRepo = batchRepo;
        _approvalRepo = approvalRepo;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var totalSkus = await _productRepo.GetQueryable().CountAsync(p => p.IsActive);

        var today = DateTime.UtcNow.Date;
        var todayTxns = await _txnRepo.GetQueryable().CountAsync(t => t.CreatedAt >= today);

        var lowStockAlerts = await _productRepo.GetQueryable().CountAsync(p => p.IsActive); // Placeholder calculation

        var warningDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var expiringBatches = await _batchRepo.GetQueryable()
            .CountAsync(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value <= warningDate && b.Status == BatchStatus.Active);

        var pendingApprovals = await _approvalRepo.GetQueryable()
            .CountAsync(a => a.Status == ApprovalStatus.Pending);

        return new DashboardSummaryDto(
            TotalActiveSkus: totalSkus,
            TodayTransactions: todayTxns,
            LowStockAlerts: lowStockAlerts,
            ExpiringBatchAlerts: expiringBatches,
            PendingApprovals: pendingApprovals
        );
    }

    public async Task<List<MovementChartDto>> GetMovementChartAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        var txns = await _txnRepo.GetQueryable()
            .Where(t => t.CreatedAt >= startDate)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<MovementChartDto>();
        for (int i = 0; i < days; i++)
        {
            var dt = startDate.AddDays(i);
            var dateOnly = DateOnly.FromDateTime(dt);

            var dayTxns = txns.Where(t => t.CreatedAt.Date == dt).ToList();
            var totalIn = dayTxns.Where(t => t.TxnType == StockTxnType.GrnIn || t.TxnType == StockTxnType.TransferIn || t.TxnType == StockTxnType.AdjIn).Sum(t => t.Quantity);
            var totalOut = dayTxns.Where(t => t.TxnType == StockTxnType.GdnOut || t.TxnType == StockTxnType.TransferOut || t.TxnType == StockTxnType.AdjOut).Sum(t => t.Quantity);

            result.Add(new MovementChartDto(dateOnly, totalIn, totalOut));
        }

        return result;
    }

    public async Task<PendingApprovalsDto> GetPendingApprovalsAsync(int approverId)
    {
        var query = _approvalRepo.GetQueryable()
            .Where(a => a.Status == ApprovalStatus.Pending);

        var count = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new PendingApprovalItemDto(
                a.Id,
                a.DocumentType.ToString(),
                a.DocumentId,
                $"{a.DocumentType}-{a.DocumentId}",
                "System User",
                a.CreatedAt
            ))
            .ToListAsync();

        return new PendingApprovalsDto(count, items);
    }

    public async Task<List<LowStockItemDto>> GetLowStockAsync(int top = 10)
    {
        var products = await _productRepo.GetQueryable()
            .Include(p => p.BinStocks)
            .Where(p => p.IsActive)
            .AsNoTracking()
            .ToListAsync();

        return products
            .Select(p =>
            {
                var totalQty = p.BinStocks.Sum(bs => bs.Quantity);
                return new LowStockItemDto(
                    p.Id, p.SKU, p.Name, totalQty, p.MinStock, Math.Max(0, p.MinStock - totalQty)
                );
            })
            .Where(x => x.TotalQty < x.MinStock)
            .OrderByDescending(x => x.ShortfallQty)
            .Take(top)
            .ToList();
    }

    public async Task<List<ExpiringBatchItemDto>> GetExpiringBatchesAsync(int withinDays = 30)
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));

        var batches = await _batchRepo.GetQueryable()
            .Include(b => b.Product)
            .Include(b => b.BinStocks)
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value <= targetDate && b.Status == BatchStatus.Active)
            .AsNoTracking()
            .OrderBy(b => b.ExpiryDate)
            .Take(10)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return batches.Select(b => new ExpiringBatchItemDto(
            b.Id,
            b.LotNumber,
            b.Product.Name,
            b.Product.SKU,
            b.ExpiryDate,
            b.ExpiryDate.HasValue ? b.ExpiryDate.Value.DayNumber - today.DayNumber : 0,
            b.BinStocks.Sum(bs => bs.Quantity)
        )).ToList();
    }

    public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 10)
    {
        var txns = await _txnRepo.GetQueryable()
            .Include(t => t.Product)
            .Include(t => t.Bin)
            .Include(t => t.CreatedByUser)
            .OrderByDescending(t => t.Id)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return txns.Select(t => new RecentActivityDto(
            t.Id,
            t.TxnType.ToString(),
            t.Product.SKU,
            t.Product.Name,
            t.Quantity,
            t.Bin.Code,
            t.CreatedByUser?.FullName ?? "System",
            t.CreatedAt
        )).ToList();
    }
}
