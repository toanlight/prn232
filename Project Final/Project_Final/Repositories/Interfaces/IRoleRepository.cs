using BusinessLayer.Entities.Identity;

namespace Repositories.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByCodeAsync(string roleCode);
}
