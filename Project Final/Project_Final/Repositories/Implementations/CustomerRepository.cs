using BusinessLayer.Entities.Partners;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    private readonly CustomerDAO _customerDao;

    public CustomerRepository(WmsDbContext context) : base(context)
    {
        _customerDao = new CustomerDAO(context);
    }

    public async Task<Customer?> GetByCodeAsync(string code) => await _customerDao.GetByCodeAsync(code);
    public async Task<(List<Customer> Items, int TotalCount)> SearchAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _customerDao.SearchAsync(keyword, isActive, pageIndex, pageSize);
}
