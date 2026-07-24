using BusinessLayer.Entities.Products;

namespace Repositories.Interfaces;

public interface ICategoryRepository : IGenericRepository<ProductCategory>
{
    Task<ProductCategory?> GetByCodeAsync(string code);
    Task<List<ProductCategory>> GetTreeCategoriesAsync();
    Task<List<ProductCategory>> GetActiveCategoriesAsync();
    Task<List<ProductCategory>> GetAllWithParentAsync();
}
