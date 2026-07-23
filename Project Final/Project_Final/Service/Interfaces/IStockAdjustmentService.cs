using BusinessLayer.Entities.Orders;

namespace Service.Interfaces;

public interface IStockAdjustmentService
{
    Task<StockAdjustment> GetByIdAsync(int id);
    Task<StockAdjustment> CreateAsync(CreateAdjustmentDto dto, int userId);
    Task SubmitAsync(int id, int userId);

    /// <summary>
    /// Gọi bởi ApprovalService sau khi được phê duyệt.
    /// Thực hiện: cân bằng BinStock, ghi StockTransaction.
    /// </summary>
    Task FinalizeAsync(int adjustmentId);
}

public record CreateAdjustmentDto(
    int WarehouseId,
    int? StockCountId,
    string? Reason,
    string? Notes,
    List<AdjustmentLineDto> Lines
);

public record AdjustmentLineDto(
    int ProductId,
    int BinId,
    int? BatchId,
    decimal AdjustQty,  // có thể âm (giảm) hoặc dương (tăng)
    string? Reason
);
