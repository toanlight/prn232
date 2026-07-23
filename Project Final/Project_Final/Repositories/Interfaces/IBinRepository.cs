using BusinessLayer.Entities.Warehouses;

namespace Repositories.Interfaces;

public interface IBinRepository : IGenericRepository<Bin>
{
    Task<List<Bin>> GetByShelfIdAsync(int shelfId);
}
