using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Orders;

namespace DataAccessLayer.DAO;

public class DispatchRequestDAO : GenericDAO<DispatchRequest>
{
    public DispatchRequestDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<DispatchRequest?> GetByRequestNumberAsync(string requestNumber)
    {
        return await _dbSet
            .Include(dr => dr.Warehouse)
            .Include(dr => dr.CreatedByUser)
            .FirstOrDefaultAsync(dr => dr.RequestNumber.ToLower() == requestNumber.ToLower());
    }
}
