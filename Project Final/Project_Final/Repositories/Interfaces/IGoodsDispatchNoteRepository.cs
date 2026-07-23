using BusinessLayer.Entities.Orders;

namespace Repositories.Interfaces;

public interface IGoodsDispatchNoteRepository : IGenericRepository<GoodsDispatchNote>
{
    Task<GoodsDispatchNote?> GetByGdnNumberAsync(string gdnNumber);
    Task<GoodsDispatchNote?> GetWithDetailsByIdAsync(int id);
}
