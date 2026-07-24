using BusinessLayer.Entities.Partners;

namespace Repositories.Interfaces;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByCodeAsync(string code);
    Task<(List<Customer> Items, int TotalCount)> SearchAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<bool> HasDispatchedGoodsAsync(int customerId);
}
