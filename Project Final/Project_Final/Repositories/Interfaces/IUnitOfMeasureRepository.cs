using BusinessLayer.Entities.Products;

namespace Repositories.Interfaces;

public interface IUnitOfMeasureRepository : IGenericRepository<UnitOfMeasure>
{
    Task<UnitOfMeasure?> GetByCodeAsync(string code);
}
