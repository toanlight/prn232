using BusinessLayer.Entities.Warehouses;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
{
    private readonly WarehouseDAO _warehouseDao;

    public WarehouseRepository(WmsDbContext context) : base(context)
    {
        _warehouseDao = new WarehouseDAO(context);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code) => await _warehouseDao.GetByCodeAsync(code);
    public async Task<Warehouse?> GetWithHierarchyByIdAsync(int id) => await _warehouseDao.GetWithHierarchyByIdAsync(id);
    public async Task<List<Warehouse>> GetActiveWarehousesAsync() => await _warehouseDao.GetActiveWarehousesAsync();
}
