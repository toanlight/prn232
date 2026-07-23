using BusinessLayer.Entities.Orders;

namespace Repositories.Interfaces;

public interface IDispatchRequestRepository : IGenericRepository<DispatchRequest>
{
    Task<DispatchRequest?> GetByRequestNumberAsync(string requestNumber);
}
