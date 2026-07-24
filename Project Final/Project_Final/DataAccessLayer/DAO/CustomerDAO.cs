using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Partners;

namespace DataAccessLayer.DAO;

public class CustomerDAO : GenericDAO<Customer>
{
    public CustomerDAO(WmsDbContext context) : base(context)
    {
    }

    public override async Task<Customer?> GetByIdAsync(int id)
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.CreatedByUser)
            .Include(c => c.UpdatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByCodeAsync(string code)
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.CreatedByUser)
            .Include(c => c.UpdatedByUser)
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task<(List<Customer> Items, int TotalCount)> SearchAsync(
        string? keyword,
        bool? isActive,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = _dbSet.AsNoTracking()
            .Include(c => c.CreatedByUser)
            .Include(c => c.UpdatedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                     c.Code.ToLower().Contains(search) ||
                                     (c.TaxCode != null && c.TaxCode.ToLower().Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasDispatchedGoodsAsync(int customerId)
    {
        var hasGDN = await _context.GoodsDispatchNotes.AnyAsync(gdn => gdn.CustomerId == customerId);
        if (hasGDN) return true;

        var hasRequest = await _context.DispatchRequests.AnyAsync(dr => dr.CustomerId == customerId);
        return hasRequest;
    }
}
