using BusinessLayer.Entities.Orders;

namespace Repositories.Interfaces;

public interface ITransferOrderRepository : IGenericRepository<TransferOrder>
{
    Task<TransferOrder?> GetByTransferNumberAsync(string transferNumber);
    Task<TransferOrder?> GetWithDetailsByIdAsync(int id);
}
