using BusinessLayer.Entities.Orders;

namespace Service.Interfaces;

public interface IPurchaseOrderService
{
    Task<(List<PurchaseOrder> Items, int TotalCount)> SearchAsync(string? poNumber, int? supplierId, int? warehouseId, string? status, int pageIndex = 1, int pageSize = 10);
    Task<PurchaseOrder> GetByIdAsync(int id);
    Task<PurchaseOrder> CreateAsync(CreatePurchaseOrderDto dto, int userId);
    Task<PurchaseOrder> UpdateAsync(int id, UpdatePurchaseOrderDto dto, int userId);
    Task SubmitAsync(int id, int userId);
    Task CancelAsync(int id, int userId);
}

public record CreatePurchaseOrderDto(
    int SupplierId,
    int WarehouseId,
    DateTime? ExpectedDate,
    string? Notes,
    List<POLineDto> Lines
);

public record UpdatePurchaseOrderDto(
    DateTime? ExpectedDate,
    string? Notes,
    List<POLineDto> Lines
);

public record POLineDto(
    int ProductId,
    decimal OrderedQty,
    decimal? UnitPrice
);
