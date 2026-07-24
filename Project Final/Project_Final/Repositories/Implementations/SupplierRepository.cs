using BusinessLayer.Entities.Partners;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
{
    private readonly SupplierDAO _supplierDao;

    public SupplierRepository(WmsDbContext context) : base(context)
    {
        _supplierDao = new SupplierDAO(context);
    }

    public async Task<Supplier?> GetByCodeAsync(string code) => await _supplierDao.GetByCodeAsync(code);
    public async Task<(List<Supplier> Items, int TotalCount)> SearchAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _supplierDao.SearchAsync(keyword, isActive, pageIndex, pageSize);
    public async Task<bool> HasSuppliedGoodsAsync(int supplierId)
        => await _supplierDao.HasSuppliedGoodsAsync(supplierId);
}
