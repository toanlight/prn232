using BusinessLayer.Entities.Orders;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class GoodsReceiptNoteRepository : GenericRepository<GoodsReceiptNote>, IGoodsReceiptNoteRepository
{
    private readonly GoodsReceiptNoteDAO _grnDao;

    public GoodsReceiptNoteRepository(WmsDbContext context) : base(context)
    {
        _grnDao = new GoodsReceiptNoteDAO(context);
    }

    public async Task<GoodsReceiptNote?> GetByGrnNumberAsync(string grnNumber) => await _grnDao.GetByGrnNumberAsync(grnNumber);
    public async Task<GoodsReceiptNote?> GetWithDetailsByIdAsync(int id) => await _grnDao.GetWithDetailsByIdAsync(id);
}
