using BusinessLayer.Entities.Orders;

namespace Service.Interfaces;

public interface IGrnService
{
    Task<(List<GoodsReceiptNote> Items, int TotalCount)> SearchAsync(string? status, int? supplierId, int? warehouseId, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10);
    Task<GoodsReceiptNote> GetByIdAsync(int id);
    Task<GoodsReceiptNote> CreateAsync(CreateGrnDto dto, int userId);
    Task<GoodsReceiptNote> UpdateAsync(int id, UpdateGrnDto dto, int userId);
    Task SubmitAsync(int id, int userId);
    Task CancelAsync(int id, int userId);

    /// <summary>
    /// Gọi bởi ApprovalService sau khi L2 phê duyệt.
    /// Thực hiện: tạo Batch, upsert BinStock, ghi StockTransaction.
    /// </summary>
    Task FinalizeAsync(int grnId);
}

public record CreateGrnDto(
    int SupplierId,
    int WarehouseId,
    int? PoId,
    DateTime ReceiptDate,
    string? Notes,
    List<GrnLineDto> Lines
);

public record UpdateGrnDto(
    DateTime ReceiptDate,
    string? Notes,
    List<GrnLineDto> Lines
);

public record GrnLineDto(
    int ProductId,
    decimal Quantity,
    int BinId,
    string LotNumber,
    DateOnly? MfgDate,
    DateOnly? ExpiryDate,
    decimal? UnitPrice
);
