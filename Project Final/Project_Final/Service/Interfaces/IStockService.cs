using BusinessLayer.Entities.Stock;

namespace Service.Interfaces;

public interface IStockService
{
    Task<List<StockSummaryDto>> GetCurrentStockAsync(int? warehouseId, int? categoryId, bool? belowMinStock);
    Task<List<BinStock>> GetStockByLocationAsync(int? warehouseId, int? zoneId, int? binId);
    Task<List<BinStock>> GetStockByBatchAsync(int? productId);
    Task<FefoSuggestionDto> GetFefoSuggestionAsync(int productId, decimal quantity, int warehouseId);
    Task<decimal> GetAvailableStockAsync(int productId);
}

public record StockSummaryDto(
    int ProductId,
    string SKU,
    string ProductName,
    string CategoryName,
    string UomCode,
    decimal TotalQuantity,
    decimal TotalAvailable,
    decimal TotalReserved,
    decimal MinStock,
    bool IsBelowMin
);

public record FefoSuggestionItemDto(
    int BatchId,
    string LotNumber,
    DateOnly? ExpiryDate,
    int BinId,
    string BinPath,
    decimal AvailableQty,
    int Priority
);

public record FefoSuggestionDto(
    int ProductId,
    decimal RequestedQty,
    decimal TotalAvailable,
    bool CanFulfill,
    List<FefoSuggestionItemDto> Suggestions
);
