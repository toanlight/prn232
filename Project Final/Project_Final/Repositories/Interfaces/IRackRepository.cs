using BusinessLayer.Entities.Warehouses;

namespace Repositories.Interfaces;

public interface IRackRepository : IGenericRepository<Rack>
{
    Task<List<Rack>> GetByZoneIdAsync(int zoneId);
}
