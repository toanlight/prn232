using System.Linq.Expressions;

namespace Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetQueryable();
    IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate);
    
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(long id);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
