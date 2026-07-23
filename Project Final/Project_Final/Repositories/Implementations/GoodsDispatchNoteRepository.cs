using BusinessLayer.Entities.Orders;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class GoodsDispatchNoteRepository : GenericRepository<GoodsDispatchNote>, IGoodsDispatchNoteRepository
{
    private readonly GoodsDispatchNoteDAO _gdnDao;

    public GoodsDispatchNoteRepository(WmsDbContext context) : base(context)
    {
        _gdnDao = new GoodsDispatchNoteDAO(context);
    }

    public async Task<GoodsDispatchNote?> GetByGdnNumberAsync(string gdnNumber) => await _gdnDao.GetByGdnNumberAsync(gdnNumber);
    public async Task<GoodsDispatchNote?> GetWithDetailsByIdAsync(int id) => await _gdnDao.GetWithDetailsByIdAsync(id);
}
