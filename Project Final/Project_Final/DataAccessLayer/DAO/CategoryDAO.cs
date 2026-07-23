using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Products;

namespace DataAccessLayer.DAO;

public class CategoryDAO : GenericDAO<ProductCategory>
{
    public CategoryDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<ProductCategory?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(c => c.Parent)
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());
    }

    public async Task<List<ProductCategory>> GetTreeCategoriesAsync()
    {
        return await _dbSet
            .Include(c => c.Children)
            .Where(c => c.ParentId == null && c.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ProductCategory>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
