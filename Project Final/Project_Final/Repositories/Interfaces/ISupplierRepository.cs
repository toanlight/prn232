using BusinessLayer.Entities.Partners;

namespace Repositories.Interfaces;

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<Supplier?> GetByCodeAsync(string code);
    Task<(List<Supplier> Items, int TotalCount)> SearchAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
}
