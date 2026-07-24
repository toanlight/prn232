using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Products;

namespace DataAccessLayer.DAO;

/// <summary>
/// Lớp ProductDAO mở rộng từ GenericDAO<Product>, cung cấp các truy vấn dữ liệu đặc thù cho Sản phẩm (Product).
/// Thao tác trực tiếp với EF Core WmsDbContext.
/// </summary>
public class ProductDAO : GenericDAO<Product>
{
    public ProductDAO(WmsDbContext context) : base(context)
    {
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm kèm theo thông tin Danh mục (Category) và Đơn vị tính (Uom)
    /// </summary>
    public async Task<List<Product>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .Include(p => p.CreatedByUser)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của 1 sản phẩm theo Id (bao gồm Category, Uom và Batches)
    /// </summary>
    public async Task<Product?> GetDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Batches)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Lấy sản phẩm theo SKU
    /// </summary>
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.SKU.ToLower() == sku.ToLower());
    }

    /// <summary>
    /// Tìm kiếm sản phẩm phân trang theo từ khóa và danh mục
    /// </summary>
    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(
        string? searchKeyword, 
        int? categoryId, 
        bool? isActive, 
        int pageIndex = 1, 
        int pageSize = 10)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .Include(p => p.CreatedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keyword = searchKeyword.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(keyword) || 
                                     p.SKU.ToLower().Contains(keyword) || 
                                     (p.Barcode != null && p.Barcode.ToLower().Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Kiểm tra sản phẩm đã có dữ liệu liên quan (lô hàng, tồn kho, đơn hàng, chứng từ) hay chưa trước khi xóa
    /// </summary>
    public async Task<bool> HasAssociatedRecordsAsync(int productId)
    {
        var hasBatches = await _context.Batches.AnyAsync(b => b.ProductId == productId);
        if (hasBatches) return true;

        var hasBinStocks = await _context.BinStocks.AnyAsync(bs => bs.ProductId == productId);
        if (hasBinStocks) return true;

        var hasPOLines = await _context.POLines.AnyAsync(pol => pol.ProductId == productId);
        if (hasPOLines) return true;

        var hasGRNLines = await _context.GRNLines.AnyAsync(grnl => grnl.ProductId == productId);
        if (hasGRNLines) return true;

        var hasGDNLines = await _context.GDNLines.AnyAsync(gdnl => gdnl.ProductId == productId);
        if (hasGDNLines) return true;

        var hasTransferLines = await _context.TransferOrderLines.AnyAsync(tol => tol.ProductId == productId);
        if (hasTransferLines) return true;

        var hasAdjLines = await _context.StockAdjustmentLines.AnyAsync(sal => sal.ProductId == productId);
        if (hasAdjLines) return true;

        var hasCountLines = await _context.StockCountLines.AnyAsync(scl => scl.ProductId == productId);
        return hasCountLines;
    }
}
