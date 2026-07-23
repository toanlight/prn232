using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Identity;

namespace DataAccessLayer.DAO;

public class RoleDAO : GenericDAO<Role>
{
    public RoleDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByCodeAsync(string roleCode)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.RoleCode.ToLower() == roleCode.ToLower());
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync();
    }
}
