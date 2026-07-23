using BusinessLayer.Entities.Stock;

namespace Service.Interfaces;

public interface IReportService
{
    Task<List<StockSummaryDto>> GetCurrentStockReportAsync(int? warehouseId, int? categoryId, bool? belowMinStock);
    Task<List<StockTransaction>> GetInventoryMovementReportAsync(DateTime? from, DateTime? to, int? warehouseId, int? productId, string? txnType);
    Task<List<Batch>> GetExpiryReportAsync(int? warehouseId, int warningDays = 30);
    Task<byte[]> ExportReportAsync(string reportType, string format, Dictionary<string, string> parameters);
}
