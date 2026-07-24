using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Partners;

namespace DataAccessLayer.DAO;

public class SupplierDAO : GenericDAO<Supplier>
{
    public SupplierDAO(WmsDbContext context) : base(context)
    {
    }

    public override async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _dbSet.AsNoTracking()
            .Include(s => s.CreatedByUser)
            .Include(s => s.UpdatedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Supplier?> GetByCodeAsync(string code)
    {
        return await _dbSet.AsNoTracking()
            .Include(s => s.CreatedByUser)
            .Include(s => s.UpdatedByUser)
            .FirstOrDefaultAsync(s => s.Code.ToLower() == code.ToLower());
    }

    public async Task<(List<Supplier> Items, int TotalCount)> SearchAsync(
        string? keyword,
        bool? isActive,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = _dbSet.AsNoTracking()
            .Include(s => s.CreatedByUser)
            .Include(s => s.UpdatedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search) ||
                                     s.Code.ToLower().Contains(search) ||
                                     (s.TaxCode != null && s.TaxCode.ToLower().Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasSuppliedGoodsAsync(int supplierId)
    {
        var hasPO = await _context.PurchaseOrders.AnyAsync(po => po.SupplierId == supplierId);
        if (hasPO) return true;

        var hasGRN = await _context.GoodsReceiptNotes.AnyAsync(grn => grn.SupplierId == supplierId);
        if (hasGRN) return true;

        var hasBatch = await _context.Batches.AnyAsync(b => b.SupplierId == supplierId);
        return hasBatch;
    }
}
