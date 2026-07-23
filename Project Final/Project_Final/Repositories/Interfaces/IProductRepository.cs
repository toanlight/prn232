using BusinessLayer.Entities.Products;

namespace Repositories.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<List<Product>> GetAllWithDetailsAsync();
    Task<Product?> GetDetailsByIdAsync(int id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<(List<Product> Items, int TotalCount)> SearchAsync(string? searchKeyword, int? categoryId, bool? isActive, int pageIndex = 1, int pageSize = 10);
}
