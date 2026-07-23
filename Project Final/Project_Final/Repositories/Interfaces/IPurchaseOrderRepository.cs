using BusinessLayer.Entities.Orders;

namespace Repositories.Interfaces;

public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetByPoNumberAsync(string poNumber);
    Task<PurchaseOrder?> GetWithDetailsByIdAsync(int id);
    Task<(List<PurchaseOrder> Items, int TotalCount)> SearchAsync(string? poNumber, int? supplierId, int? warehouseId, string? status, int pageIndex = 1, int pageSize = 10);
}
