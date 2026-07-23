using BusinessLayer.Entities.Orders;

namespace Service.Interfaces;

public interface IGdnService
{
    Task<(List<GoodsDispatchNote> Items, int TotalCount)> SearchAsync(string? status, int? customerId, int? warehouseId, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10);
    Task<GoodsDispatchNote> GetByIdAsync(int id);
    Task<GoodsDispatchNote> CreateAsync(CreateGdnDto dto, int userId);
    Task<GoodsDispatchNote> UpdateAsync(int id, UpdateGdnDto dto, int userId);
    Task SubmitAsync(int id, int userId);
    Task CancelAsync(int id, int userId);
    Task StartPickingAsync(int id, int userId);
    Task ConfirmPickedAsync(int id, int userId);
    Task DeliverAsync(int id, int userId, DeliverGdnDto dto);

    /// <summary>
    /// Gọi bởi ApprovalService sau khi L2 phê duyệt.
    /// Chuyển trạng thái GDN sang APPROVED.
    /// </summary>
    Task FinalizeAsync(int gdnId);
}

public record CreateGdnDto(
    int CustomerId,
    int WarehouseId,
    int? DispatchRequestId,
    DateTime? DispatchDate,
    string? Notes,
    List<GdnLineDto> Lines
);

public record UpdateGdnDto(
    DateTime? DispatchDate,
    string? Notes,
    List<GdnLineDto> Lines
);

public record GdnLineDto(
    int ProductId,
    int BinId,
    int? BatchId,
    decimal Quantity
);

public record DeliverGdnDto(
    string? DeliveryNote,
    List<ActualLineDto> ActualLines
);

public record ActualLineDto(int GdnLineId, decimal PickedQty);
