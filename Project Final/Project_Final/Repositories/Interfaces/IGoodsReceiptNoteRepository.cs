using BusinessLayer.Entities.Orders;

namespace Repositories.Interfaces;

public interface IGoodsReceiptNoteRepository : IGenericRepository<GoodsReceiptNote>
{
    Task<GoodsReceiptNote?> GetByGrnNumberAsync(string grnNumber);
    Task<GoodsReceiptNote?> GetWithDetailsByIdAsync(int id);
}
