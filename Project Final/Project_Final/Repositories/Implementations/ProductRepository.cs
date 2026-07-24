using BusinessLayer.Entities.Products;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly ProductDAO _productDao;

    public ProductRepository(WmsDbContext context) : base(context)
    {
        _productDao = new ProductDAO(context);
    }

    public override async Task<Product?> GetByIdAsync(int id) => await _productDao.GetByIdAsync(id);
    public async Task<List<Product>> GetAllWithDetailsAsync() => await _productDao.GetAllWithDetailsAsync();
    public async Task<Product?> GetDetailsByIdAsync(int id) => await _productDao.GetDetailsByIdAsync(id);
    public async Task<Product?> GetBySkuAsync(string sku) => await _productDao.GetBySkuAsync(sku);
    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(string? searchKeyword, int? categoryId, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _productDao.SearchAsync(searchKeyword, categoryId, isActive, pageIndex, pageSize);

    public async Task<bool> HasAssociatedRecordsAsync(int productId)
        => await _productDao.HasAssociatedRecordsAsync(productId);
}
