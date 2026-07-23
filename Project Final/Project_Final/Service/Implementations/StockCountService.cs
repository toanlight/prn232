using BusinessLayer.Entities.Orders;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class StockCountService : IStockCountService
{
    private readonly IStockCountRepository _countRepo;
    private readonly IBinStockRepository _binStockRepo;
    private readonly IWarehouseRepository _warehouseRepo;

    public StockCountService(
        IStockCountRepository countRepo,
        IBinStockRepository binStockRepo,
        IWarehouseRepository warehouseRepo)
    {
        _countRepo = countRepo;
        _binStockRepo = binStockRepo;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<StockCount> GetByIdAsync(int id)
    {
        var count = await _countRepo.GetQueryable()
            .Include(c => c.Warehouse)
            .Include(c => c.PlannedByUser)
            .Include(c => c.Lines)
                .ThenInclude(l => l.Product)
            .Include(c => c.Lines)
                .ThenInclude(l => l.Batch)
            .Include(c => c.Lines)
                .ThenInclude(l => l.Bin)
            .FirstOrDefaultAsync(c => c.Id == id);

        return count ?? throw new KeyNotFoundException($"Không tìm thấy kế hoạch kiểm kê ID={id}.");
    }

    public async Task<StockCount> CreateAsync(CreateStockCountDto dto, int userId)
    {
        if (!await _warehouseRepo.ExistsAsync(w => w.Id == dto.WarehouseId))
            throw new KeyNotFoundException($"Không tìm thấy kho ID={dto.WarehouseId}.");

        if (!Enum.TryParse<CountType>(dto.CountType, true, out var countType))
            throw new ArgumentException($"Loại kiểm kê '{dto.CountType}' không hợp lệ.");

        // Snapshot SystemQty từ BinStock hiện tại trong kho
        var stockQuery = _binStockRepo.GetQueryable()
            .Where(bs => bs.Bin.Shelf.Rack.Zone.WarehouseId == dto.WarehouseId && bs.Quantity > 0);

        if (dto.BinIds != null && dto.BinIds.Count > 0)
            stockQuery = stockQuery.Where(bs => dto.BinIds.Contains(bs.BinId));

        var currentStocks = await stockQuery.ToListAsync();

        var countNumber = $"CNT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var count = new StockCount
        {
            CountNumber = countNumber,
            WarehouseId = dto.WarehouseId,
            CountType = countType,
            CountDate = DateOnly.FromDateTime(dto.PlannedDate),
            PlannedBy = userId,
            Notes = dto.Notes,
            Status = "PLANNED",
            CreatedAt = DateTime.UtcNow,
            Lines = currentStocks.Select(bs => new StockCountLine
            {
                BinId = bs.BinId,
                ProductId = bs.ProductId,
                BatchId = bs.BatchId,
                SystemQty = bs.Quantity,
                ActualQty = null
            }).ToList()
        };

        await _countRepo.AddAsync(count);
        await _countRepo.SaveChangesAsync();
        return count;
    }

    public async Task StartCountAsync(int id, int userId)
    {
        var count = await GetByIdAsync(id);
        if (count.Status != "PLANNED")
            throw new InvalidOperationException("Chỉ kiểm kê ở trạng thái PLANNED mới có thể bắt đầu.");

        count.Status = "IN_PROGRESS";
        count.StartedAt = DateTime.UtcNow;
        _countRepo.Update(count);
        await _countRepo.SaveChangesAsync();
    }

    public async Task UpdateLineActualQtyAsync(int countId, int lineId, decimal actualQty, int userId)
    {
        var count = await GetByIdAsync(countId);
        if (count.Status != "IN_PROGRESS")
            throw new InvalidOperationException("Chỉ có thể nhập số lượng thực tế khi đợt kiểm kê đang IN_PROGRESS.");

        var line = count.Lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new KeyNotFoundException($"Không tìm thấy dòng kiểm kê ID={lineId}.");

        line.ActualQty = actualQty;
        line.CountedBy = userId;
        line.CountedAt = DateTime.UtcNow;

        _countRepo.Update(count);
        await _countRepo.SaveChangesAsync();
    }

    public async Task CompleteCountAsync(int id, int userId)
    {
        var count = await GetByIdAsync(id);
        if (count.Status != "IN_PROGRESS")
            throw new InvalidOperationException("Chỉ đợt kiểm kê đang IN_PROGRESS mới có thể hoàn thành.");

        count.Status = "COMPLETED";
        count.CompletedAt = DateTime.UtcNow;
        _countRepo.Update(count);
        await _countRepo.SaveChangesAsync();
    }

    public async Task<List<StockCountLine>> GetVariancesAsync(int countId)
    {
        var count = await GetByIdAsync(countId);
        return count.Lines.Where(l => l.ActualQty.HasValue && l.Variance != 0).ToList();
    }
}
