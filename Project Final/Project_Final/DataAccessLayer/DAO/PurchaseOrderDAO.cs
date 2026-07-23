using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Orders;

namespace DataAccessLayer.DAO;

public class PurchaseOrderDAO : GenericDAO<PurchaseOrder>
{
    public PurchaseOrderDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<PurchaseOrder?> GetByPoNumberAsync(string poNumber)
    {
        return await _dbSet
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.CreatedByUser)
            .Include(po => po.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(po => po.PONumber.ToLower() == poNumber.ToLower());
    }

    public async Task<PurchaseOrder?> GetWithDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.CreatedByUser)
            .Include(po => po.Lines)
                .ThenInclude(l => l.Product)
            .Include(po => po.GRNs)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<(List<PurchaseOrder> Items, int TotalCount)> SearchAsync(
        string? poNumber,
        int? supplierId,
        int? warehouseId,
        string? status,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = _dbSet
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(poNumber))
        {
            var keyword = poNumber.Trim().ToLower();
            query = query.Where(po => po.PONumber.ToLower().Contains(keyword));
        }

        if (supplierId.HasValue)
        {
            query = query.Where(po => po.SupplierId == supplierId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(po => po.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(po => po.Status == status);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(po => po.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
