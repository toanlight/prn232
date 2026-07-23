using BusinessLayer.Entities.Orders;

namespace Service.Interfaces;

public interface IStockCountService
{
    Task<StockCount> GetByIdAsync(int id);
    Task<StockCount> CreateAsync(CreateStockCountDto dto, int userId);
    Task StartCountAsync(int id, int userId);
    Task UpdateLineActualQtyAsync(int countId, int lineId, decimal actualQty, int userId);
    Task CompleteCountAsync(int id, int userId);
    Task<List<StockCountLine>> GetVariancesAsync(int countId);
}

public record CreateStockCountDto(
    int WarehouseId,
    string CountType,
    DateTime PlannedDate,
    string? Notes,
    List<int>? BinIds  // null = toàn bộ kho
);
