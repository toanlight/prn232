using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Partners;

namespace DataAccessLayer.DAO;

public class CustomerDAO : GenericDAO<Customer>
{
    public CustomerDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task<(List<Customer> Items, int TotalCount)> SearchAsync(
        string? keyword,
        bool? isActive,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

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
}
