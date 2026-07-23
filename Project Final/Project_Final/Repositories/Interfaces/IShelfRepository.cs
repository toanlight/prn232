using BusinessLayer.Entities.Warehouses;

namespace Repositories.Interfaces;

public interface IShelfRepository : IGenericRepository<Shelf>
{
    Task<List<Shelf>> GetByRackIdAsync(int rackId);
}
