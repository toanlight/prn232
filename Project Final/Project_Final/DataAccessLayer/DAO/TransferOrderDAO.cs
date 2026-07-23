using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Orders;

namespace DataAccessLayer.DAO;

public class TransferOrderDAO : GenericDAO<TransferOrder>
{
    public TransferOrderDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<TransferOrder?> GetByTransferNumberAsync(string transferNumber)
    {
        return await _dbSet
            .Include(to => to.Warehouse)
            .Include(to => to.CreatedByUser)
            .Include(to => to.Lines)
                .ThenInclude(l => l.Product)
            .Include(to => to.Lines)
                .ThenInclude(l => l.FromBin)
            .Include(to => to.Lines)
                .ThenInclude(l => l.ToBin)
            .FirstOrDefaultAsync(to => to.TransferNumber.ToLower() == transferNumber.ToLower());
    }

    public async Task<TransferOrder?> GetWithDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(to => to.Warehouse)
            .Include(to => to.CreatedByUser)
            .Include(to => to.Lines)
                .ThenInclude(l => l.Product)
            .Include(to => to.Lines)
                .ThenInclude(l => l.FromBin)
            .Include(to => to.Lines)
                .ThenInclude(l => l.ToBin)
            .FirstOrDefaultAsync(to => to.Id == id);
    }
}
