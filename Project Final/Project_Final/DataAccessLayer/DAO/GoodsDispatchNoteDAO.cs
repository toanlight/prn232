using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Orders;

namespace DataAccessLayer.DAO;

public class GoodsDispatchNoteDAO : GenericDAO<GoodsDispatchNote>
{
    public GoodsDispatchNoteDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<GoodsDispatchNote?> GetByGdnNumberAsync(string gdnNumber)
    {
        return await _dbSet
            .Include(gdn => gdn.Customer)
            .Include(gdn => gdn.Warehouse)
            .Include(gdn => gdn.CreatedByUser)
            .Include(gdn => gdn.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(gdn => gdn.GDNNumber.ToLower() == gdnNumber.ToLower());
    }

    public async Task<GoodsDispatchNote?> GetWithDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(gdn => gdn.Customer)
            .Include(gdn => gdn.Warehouse)
            .Include(gdn => gdn.CreatedByUser)
            .Include(gdn => gdn.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(gdn => gdn.Id == id);
    }
}
