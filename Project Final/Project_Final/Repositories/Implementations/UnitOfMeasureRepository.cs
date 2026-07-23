using BusinessLayer.Entities.Products;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class UnitOfMeasureRepository : GenericRepository<UnitOfMeasure>, IUnitOfMeasureRepository
{
    private readonly UnitOfMeasureDAO _uomDao;

    public UnitOfMeasureRepository(WmsDbContext context) : base(context)
    {
        _uomDao = new UnitOfMeasureDAO(context);
    }

    public async Task<UnitOfMeasure?> GetByCodeAsync(string code) => await _uomDao.GetByCodeAsync(code);
}
