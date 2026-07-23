using BusinessLayer.Entities.Orders;

namespace Service.Interfaces;

public interface ITransferOrderService
{
    Task<TransferOrder> GetByIdAsync(int id);
    Task<TransferOrder> CreateAsync(CreateTransferDto dto, int userId);
    Task SubmitAsync(int id, int userId);
    Task CancelAsync(int id, int userId);

    /// <summary>
    /// Gọi bởi ApprovalService sau khi được phê duyệt.
    /// Thực hiện: debit FromBin, credit ToBin, ghi 2 StockTransaction.
    /// </summary>
    Task FinalizeAsync(int transferId);
}

public record CreateTransferDto(
    int WarehouseId,
    string? Notes,
    List<TransferLineDto> Lines
);

public record TransferLineDto(
    int ProductId,
    int? BatchId,
    int FromBinId,
    int ToBinId,
    decimal Quantity
);
