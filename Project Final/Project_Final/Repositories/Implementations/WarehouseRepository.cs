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

    public override async Task<Warehouse?> GetByIdAsync(int id) => await _warehouseDao.GetByIdAsync(id);
    public async Task<Warehouse?> GetByCodeAsync(string code) => await _warehouseDao.GetByCodeAsync(code);
    public async Task<Warehouse?> GetWithHierarchyByIdAsync(int id) => await _warehouseDao.GetWithHierarchyByIdAsync(id);
    public async Task<List<Warehouse>> GetActiveWarehousesAsync() => await _warehouseDao.GetActiveWarehousesAsync();
    public async Task<(List<Warehouse> Items, int TotalCount)> SearchAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _warehouseDao.SearchAsync(keyword, isActive, pageIndex, pageSize);
    public async Task<bool> HasAssociatedRecordsAsync(int warehouseId) => await _warehouseDao.HasAssociatedRecordsAsync(warehouseId);
}
