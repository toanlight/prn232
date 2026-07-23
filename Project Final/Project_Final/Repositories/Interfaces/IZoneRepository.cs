using BusinessLayer.Entities.Warehouses;

namespace Repositories.Interfaces;

public interface IZoneRepository : IGenericRepository<Zone>
{
    Task<List<Zone>> GetByWarehouseIdAsync(int warehouseId);
}
