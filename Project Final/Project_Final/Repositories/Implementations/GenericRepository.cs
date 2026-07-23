using System.Linq.Expressions;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly GenericDAO<T> _dao;

    public GenericRepository(WmsDbContext context)
    {
        _dao = new GenericDAO<T>(context);
    }

    public virtual IQueryable<T> GetQueryable() => _dao.GetQueryable();
    public virtual IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate) => _dao.GetQueryable(predicate);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dao.GetAllAsync();
    public virtual async Task<T?> GetByIdAsync(int id) => await _dao.GetByIdAsync(id);
    public virtual async Task<T?> GetByIdAsync(long id) => await _dao.GetByIdAsync(id);
    public virtual async Task<T?> FindAsync(Expression<Func<T, bool>> predicate) => await _dao.FindAsync(predicate);

    public virtual async Task AddAsync(T entity) => await _dao.AddAsync(entity);
    public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dao.AddRangeAsync(entities);

    public virtual void Update(T entity) => _dao.Update(entity);
    public virtual void UpdateRange(IEnumerable<T> entities) => _dao.UpdateRange(entities);

    public virtual void Remove(T entity) => _dao.Remove(entity);
    public virtual void RemoveRange(IEnumerable<T> entities) => _dao.RemoveRange(entities);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) => await _dao.CountAsync(predicate);
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => await _dao.ExistsAsync(predicate);
    public virtual async Task<int> SaveChangesAsync() => await _dao.SaveChangesAsync();
}
