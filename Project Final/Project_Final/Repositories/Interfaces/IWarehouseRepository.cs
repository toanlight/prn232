using BusinessLayer.Entities.Warehouses;

namespace Repositories.Interfaces;

public interface IWarehouseRepository : IGenericRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(string code);
    Task<Warehouse?> GetWithHierarchyByIdAsync(int id);
    Task<List<Warehouse>> GetActiveWarehousesAsync();
    Task<(List<Warehouse> Items, int TotalCount)> SearchAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<bool> HasAssociatedRecordsAsync(int warehouseId);
}
