using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Partners;

namespace DataAccessLayer.DAO;

public class SupplierDAO : GenericDAO<Supplier>
{
    public SupplierDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<Supplier?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Code.ToLower() == code.ToLower());
    }

    public async Task<(List<Supplier> Items, int TotalCount)> SearchAsync(
        string? keyword,
        bool? isActive,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

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
}
