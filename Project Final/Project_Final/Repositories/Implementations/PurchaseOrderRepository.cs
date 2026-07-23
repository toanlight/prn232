using BusinessLayer.Entities.Orders;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class PurchaseOrderRepository : GenericRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    private readonly PurchaseOrderDAO _poDao;

    public PurchaseOrderRepository(WmsDbContext context) : base(context)
    {
        _poDao = new PurchaseOrderDAO(context);
    }

    public async Task<PurchaseOrder?> GetByPoNumberAsync(string poNumber) => await _poDao.GetByPoNumberAsync(poNumber);
    public async Task<PurchaseOrder?> GetWithDetailsByIdAsync(int id) => await _poDao.GetWithDetailsByIdAsync(id);
    public async Task<(List<PurchaseOrder> Items, int TotalCount)> SearchAsync(string? poNumber, int? supplierId, int? warehouseId, string? status, int pageIndex = 1, int pageSize = 10)
        => await _poDao.SearchAsync(poNumber, supplierId, warehouseId, status, pageIndex, pageSize);
}
