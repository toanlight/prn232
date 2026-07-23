using BusinessLayer.Entities.Orders;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class DispatchRequestRepository : GenericRepository<DispatchRequest>, IDispatchRequestRepository
{
    private readonly DispatchRequestDAO _drDao;

    public DispatchRequestRepository(WmsDbContext context) : base(context)
    {
        _drDao = new DispatchRequestDAO(context);
    }

    public async Task<DispatchRequest?> GetByRequestNumberAsync(string requestNumber) => await _drDao.GetByRequestNumberAsync(requestNumber);
}
