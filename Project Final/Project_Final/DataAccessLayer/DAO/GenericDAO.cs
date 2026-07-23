using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.DAO;

/// <summary>
/// Lớp DAO dùng chung (Generic Data Access Object) cho các Entity,
/// chịu trách nhiệm thực hiện các thao tác CRUD trực tiếp xuống cơ sở dữ liệu qua WmsDbContext.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu Entity</typeparam>
public class GenericDAO<T> where T : class
{
    protected readonly WmsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericDAO(WmsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Lấy IQueryable để tùy biến truy vấn (paging, filtering, sorting, include...)
    /// </summary>
    public virtual IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    /// <summary>
    /// Lấy IQueryable có kèm điều kiện lọc (predicate)
    /// </summary>
    public virtual IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    /// <summary>
    /// Lấy danh sách toàn bộ bản ghi bất đồng bộ
    /// </summary>
    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Lấy bản ghi theo khoá chính Id (kiểu int)
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Lấy bản ghi theo khoá chính Id (kiểu long)
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Tìm kiếm bản ghi đầu tiên thỏa mãn điều kiện
    /// </summary>
    public virtual async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Thêm mới 1 bản ghi vào DbContext
    /// </summary>
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Thêm mới nhiều bản ghi vào DbContext
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// Cập nhật thông tin bản ghi trong DbContext
    /// </summary>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Cập nhật nhiều bản ghi trong DbContext
    /// </summary>
    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Xóa 1 bản ghi khỏi DbContext
    /// </summary>
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Xóa nhiều bản ghi khỏi DbContext
    /// </summary>
    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Đếm số bản ghi (có thể kèm điều kiện)
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null 
            ? await _dbSet.CountAsync() 
            : await _dbSet.CountAsync(predicate);
    }

    /// <summary>
    /// Kiểm tra sự tồn tại của bản ghi theo điều kiện
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <summary>
    /// Lưu các thay đổi xuống Database
    /// </summary>
    public virtual async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
