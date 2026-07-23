using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Products;

namespace DataAccessLayer.DAO;

public class UnitOfMeasureDAO : GenericDAO<UnitOfMeasure>
{
    public UnitOfMeasureDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<UnitOfMeasure?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Code.ToLower() == code.ToLower());
    }

    public async Task<List<UnitOfMeasure>> GetActiveUomsAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
