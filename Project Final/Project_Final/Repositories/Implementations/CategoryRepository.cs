using BusinessLayer.Entities.Products;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class CategoryRepository : GenericRepository<ProductCategory>, ICategoryRepository
{
    private readonly CategoryDAO _categoryDao;

    public CategoryRepository(WmsDbContext context) : base(context)
    {
        _categoryDao = new CategoryDAO(context);
    }

    public async Task<ProductCategory?> GetByCodeAsync(string code) => await _categoryDao.GetByCodeAsync(code);
    public async Task<List<ProductCategory>> GetTreeCategoriesAsync() => await _categoryDao.GetTreeCategoriesAsync();
    public async Task<List<ProductCategory>> GetActiveCategoriesAsync() => await _categoryDao.GetActiveCategoriesAsync();
}
