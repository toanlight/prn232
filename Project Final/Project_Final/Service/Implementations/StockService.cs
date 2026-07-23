using BusinessLayer.Entities.Stock;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class StockService : IStockService
{
    private readonly IBinStockRepository _binStockRepo;
    private readonly IBatchRepository _batchRepo;

    public StockService(IBinStockRepository binStockRepo, IBatchRepository batchRepo)
    {
        _binStockRepo = binStockRepo;
        _batchRepo = batchRepo;
    }

    public async Task<List<StockSummaryDto>> GetCurrentStockAsync(int? warehouseId, int? categoryId, bool? belowMinStock)
    {
        var query = _binStockRepo.GetQueryable()
            .Include(bs => bs.Product)
                .ThenInclude(p => p.Category)
            .Include(bs => bs.Product)
                .ThenInclude(p => p.Uom)
            .Include(bs => bs.Bin)
                .ThenInclude(b => b.Shelf)
                    .ThenInclude(s => s.Rack)
                        .ThenInclude(r => r.Zone)
            .AsNoTracking();

        if (warehouseId.HasValue)
            query = query.Where(bs => bs.Bin.Shelf.Rack.Zone.WarehouseId == warehouseId.Value);

        if (categoryId.HasValue)
            query = query.Where(bs => bs.Product.CategoryId == categoryId.Value);

        // Load then group in memory to avoid anonymous type property name collision
        var allStocks = await query.ToListAsync();

        var grouped = allStocks
            .GroupBy(bs => bs.ProductId)
            .Select(g =>
            {
                var sample = g.First();
                var totalQty = g.Sum(x => x.Quantity);
                return new StockSummaryDto(
                    ProductId: g.Key,
                    SKU: sample.Product.SKU,
                    ProductName: sample.Product.Name,
                    CategoryName: sample.Product.Category?.Name ?? "",
                    UomCode: sample.Product.Uom?.Code ?? "",
                    TotalQuantity: totalQty,
                    TotalAvailable: g.Sum(x => x.Quantity - x.ReservedQty),
                    TotalReserved: g.Sum(x => x.ReservedQty),
                    MinStock: sample.Product.MinStock,
                    IsBelowMin: totalQty < sample.Product.MinStock
                );
            })
            .ToList();

        if (belowMinStock == true)
            grouped = grouped.Where(s => s.IsBelowMin).ToList();

        return grouped;
    }

    public async Task<List<BinStock>> GetStockByLocationAsync(int? warehouseId, int? zoneId, int? binId)
    {
        var query = _binStockRepo.GetQueryable()
            .Include(bs => bs.Product)
            .Include(bs => bs.Batch)
            .Include(bs => bs.Bin)
                .ThenInclude(b => b.Shelf)
                    .ThenInclude(s => s.Rack)
                        .ThenInclude(r => r.Zone)
            .AsNoTracking();

        if (binId.HasValue)
            query = query.Where(bs => bs.BinId == binId.Value);
        else if (zoneId.HasValue)
            query = query.Where(bs => bs.Bin.Shelf.Rack.ZoneId == zoneId.Value);
        else if (warehouseId.HasValue)
            query = query.Where(bs => bs.Bin.Shelf.Rack.Zone.WarehouseId == warehouseId.Value);

        return await query.Where(bs => bs.Quantity > 0).ToListAsync();
    }

    public async Task<List<BinStock>> GetStockByBatchAsync(int? productId)
    {
        var query = _binStockRepo.GetQueryable()
            .Include(bs => bs.Product)
            .Include(bs => bs.Batch)
            .Include(bs => bs.Bin)
            .Where(bs => bs.Quantity > 0)
            .AsNoTracking();

        if (productId.HasValue)
            query = query.Where(bs => bs.ProductId == productId.Value);

        return await query.OrderBy(bs => bs.Batch!.ExpiryDate).ToListAsync();
    }

    /// <summary>
    /// FEFO Algorithm: First Expired, First Out
    /// Gợi ý các Batch+Bin theo thứ tự hạn sử dụng gần nhất để xuất đủ số lượng yêu cầu.
    /// </summary>
    public async Task<FefoSuggestionDto> GetFefoSuggestionAsync(int productId, decimal quantity, int warehouseId)
    {
        // Lấy tất cả BinStock có AvailableQty > 0 của sản phẩm trong kho
        var stocks = await _binStockRepo.GetQueryable()
            .Include(bs => bs.Batch)
            .Include(bs => bs.Bin)
                .ThenInclude(b => b.Shelf)
                    .ThenInclude(s => s.Rack)
                        .ThenInclude(r => r.Zone)
            .Where(bs =>
                bs.ProductId == productId &&
                bs.Quantity - bs.ReservedQty > 0 &&
                bs.Bin.Shelf.Rack.Zone.WarehouseId == warehouseId)
            .AsNoTracking()
            .ToListAsync();

        // Sắp xếp FEFO: ExpiryDate ASC (null xuống cuối)
        var sorted = stocks
            .OrderBy(bs => bs.Batch?.ExpiryDate == null ? DateOnly.MaxValue : bs.Batch.ExpiryDate.Value)
            .ThenBy(bs => bs.BinId)
            .ToList();

        var suggestions = new List<FefoSuggestionItemDto>();
        decimal remaining = quantity;
        int priority = 1;

        foreach (var stock in sorted)
        {
            if (remaining <= 0) break;

            var available = stock.Quantity - stock.ReservedQty;
            var takeQty = Math.Min(available, remaining);

            var binPath = BuildBinPath(stock.Bin);
            suggestions.Add(new FefoSuggestionItemDto(
                BatchId: stock.BatchId,
                LotNumber: stock.Batch?.LotNumber ?? "N/A",
                ExpiryDate: stock.Batch?.ExpiryDate,
                BinId: stock.BinId,
                BinPath: binPath,
                AvailableQty: takeQty,
                Priority: priority++
            ));

            remaining -= takeQty;
        }

        var totalAvailable = sorted.Sum(s => s.Quantity - s.ReservedQty);
        return new FefoSuggestionDto(
            ProductId: productId,
            RequestedQty: quantity,
            TotalAvailable: totalAvailable,
            CanFulfill: remaining <= 0,
            Suggestions: suggestions
        );
    }

    public async Task<decimal> GetAvailableStockAsync(int productId)
    {
        var stocks = await _binStockRepo.GetQueryable()
            .Where(bs => bs.ProductId == productId)
            .ToListAsync();
        return stocks.Sum(bs => bs.Quantity - bs.ReservedQty);
    }

    private static string BuildBinPath(BusinessLayer.Entities.Warehouses.Bin bin)
    {
        try
        {
            var shelf = bin.Shelf;
            var rack = shelf?.Rack;
            var zone = rack?.Zone;
            return $"{zone?.Code}-{rack?.Code}-{shelf?.Code}-{bin.Code}";
        }
        catch
        {
            return bin.Code;
        }
    }
}
