using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Warehouses;

namespace DataAccessLayer.DAO;

public class WarehouseDAO : GenericDAO<Warehouse>
{
    public WarehouseDAO(WmsDbContext context) : base(context)
    {
    }

    public override async Task<Warehouse?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(w => w.ManagerUser)
            .Include(w => w.Zones)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(w => w.ManagerUser)
            .Include(w => w.Zones)
            .FirstOrDefaultAsync(w => w.Code.ToLower() == code.ToLower());
    }

    public async Task<Warehouse?> GetWithHierarchyByIdAsync(int id)
    {
        return await _dbSet
            .Include(w => w.ManagerUser)
            .Include(w => w.Zones)
                .ThenInclude(z => z.Racks)
                    .ThenInclude(r => r.Shelves)
                        .ThenInclude(s => s.Bins)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<List<Warehouse>> GetActiveWarehousesAsync()
    {
        return await _dbSet
            .Include(w => w.ManagerUser)
            .Where(w => w.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<Warehouse> Items, int TotalCount)> SearchAsync(
        string? keyword,
        bool? isActive,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = _dbSet
            .Include(w => w.ManagerUser)
            .Include(w => w.Zones)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim().ToLower();
            query = query.Where(w => w.Name.ToLower().Contains(search) ||
                                     w.Code.ToLower().Contains(search) ||
                                     (w.Address != null && w.Address.ToLower().Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(w => w.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(w => w.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasAssociatedRecordsAsync(int warehouseId)
    {
        var hasZones = await _context.Zones.AnyAsync(z => z.WarehouseId == warehouseId);
        if (hasZones) return true;

        var hasPO = await _context.PurchaseOrders.AnyAsync(po => po.WarehouseId == warehouseId);
        if (hasPO) return true;

        var hasGRN = await _context.GoodsReceiptNotes.AnyAsync(grn => grn.WarehouseId == warehouseId);
        if (hasGRN) return true;

        var hasGDN = await _context.GoodsDispatchNotes.AnyAsync(gdn => gdn.WarehouseId == warehouseId);
        if (hasGDN) return true;

        var hasTransfer = await _context.TransferOrders.AnyAsync(to => to.WarehouseId == warehouseId);
        if (hasTransfer) return true;

        var hasRequest = await _context.DispatchRequests.AnyAsync(dr => dr.WarehouseId == warehouseId);
        if (hasRequest) return true;

        var hasAdjustment = await _context.StockAdjustments.AnyAsync(sa => sa.WarehouseId == warehouseId);
        if (hasAdjustment) return true;

        var hasCount = await _context.StockCounts.AnyAsync(sc => sc.WarehouseId == warehouseId);
        return hasCount;
    }
}
