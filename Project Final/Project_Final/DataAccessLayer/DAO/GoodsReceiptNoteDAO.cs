using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Orders;

namespace DataAccessLayer.DAO;

public class GoodsReceiptNoteDAO : GenericDAO<GoodsReceiptNote>
{
    public GoodsReceiptNoteDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<GoodsReceiptNote?> GetByGrnNumberAsync(string grnNumber)
    {
        return await _dbSet
            .Include(grn => grn.PO)
            .Include(grn => grn.Warehouse)
            .Include(grn => grn.CreatedByUser)
            .Include(grn => grn.Lines)
                .ThenInclude(l => l.Product)
            .Include(grn => grn.Lines)
                .ThenInclude(l => l.Bin)
            .FirstOrDefaultAsync(grn => grn.GRNNumber.ToLower() == grnNumber.ToLower());
    }

    public async Task<GoodsReceiptNote?> GetWithDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(grn => grn.PO)
            .Include(grn => grn.Warehouse)
            .Include(grn => grn.CreatedByUser)
            .Include(grn => grn.Lines)
                .ThenInclude(l => l.Product)
            .Include(grn => grn.Lines)
                .ThenInclude(l => l.Bin)
            .FirstOrDefaultAsync(grn => grn.Id == id);
    }
}
