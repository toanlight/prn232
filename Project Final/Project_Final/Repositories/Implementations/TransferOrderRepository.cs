using BusinessLayer.Entities.Orders;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class TransferOrderRepository : GenericRepository<TransferOrder>, ITransferOrderRepository
{
    private readonly TransferOrderDAO _transferDao;

    public TransferOrderRepository(WmsDbContext context) : base(context)
    {
        _transferDao = new TransferOrderDAO(context);
    }

    public async Task<TransferOrder?> GetByTransferNumberAsync(string transferNumber) => await _transferDao.GetByTransferNumberAsync(transferNumber);
    public async Task<TransferOrder?> GetWithDetailsByIdAsync(int id) => await _transferDao.GetWithDetailsByIdAsync(id);
}
