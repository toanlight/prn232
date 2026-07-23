using BusinessLayer.Entities.Identity;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    private readonly RoleDAO _roleDao;

    public RoleRepository(WmsDbContext context) : base(context)
    {
        _roleDao = new RoleDAO(context);
    }

    public async Task<Role?> GetByCodeAsync(string roleCode) => await _roleDao.GetByCodeAsync(roleCode);
}
