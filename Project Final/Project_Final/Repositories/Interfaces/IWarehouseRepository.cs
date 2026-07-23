using BusinessLayer.Entities.Warehouses;

namespace Repositories.Interfaces;

public interface IWarehouseRepository : IGenericRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(string code);
    Task<Warehouse?> GetWithHierarchyByIdAsync(int id);
    Task<List<Warehouse>> GetActiveWarehousesAsync();
}
