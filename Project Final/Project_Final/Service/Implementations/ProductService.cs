using BusinessLayer.Entities.Products;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IUnitOfMeasureRepository _uomRepo;

    public ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo, IUnitOfMeasureRepository uomRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _uomRepo = uomRepo;
    }

    public async Task<(List<Product> Items, int TotalCount)> SearchProductsAsync(
        string? keyword, int? categoryId, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _productRepo.SearchAsync(keyword, categoryId, isActive, pageIndex, pageSize);

    public async Task<Product> GetByIdAsync(int id)
        => await _productRepo.GetDetailsByIdAsync(id)
           ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID={id}.");

    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        if (await _productRepo.ExistsAsync(p => p.SKU.ToLower() == dto.SKU.ToLower()))
            throw new InvalidOperationException($"SKU '{dto.SKU}' đã tồn tại.");
        if (dto.Barcode != null && await _productRepo.ExistsAsync(p => p.Barcode == dto.Barcode))
            throw new InvalidOperationException($"Barcode '{dto.Barcode}' đã tồn tại.");
        if (!await _categoryRepo.ExistsAsync(c => c.Id == dto.CategoryId))
            throw new KeyNotFoundException($"Không tìm thấy danh mục ID={dto.CategoryId}.");
        if (!await _uomRepo.ExistsAsync(u => u.Id == dto.UomId))
            throw new KeyNotFoundException($"Không tìm thấy đơn vị tính ID={dto.UomId}.");

        var product = new Product
        {
            SKU = dto.SKU.ToUpper(),
            Name = dto.Name,
            NameEn = dto.NameEn,
            Barcode = dto.Barcode,
            CategoryId = dto.CategoryId,
            UomId = dto.UomId,
            Description = dto.Description,
            MinStock = dto.MinStock,
            ReorderPoint = dto.ReorderPoint,
            IsBatchTracked = dto.IsBatchTracked,
            IsExpiryTracked = dto.IsExpiryTracked,
            ExpiryWarningDays = dto.ExpiryWarningDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1  // TODO: Pass from HttpContext
        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _productRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID={id}.");

        if (dto.Barcode != null && await _productRepo.ExistsAsync(p => p.Barcode == dto.Barcode && p.Id != id))
            throw new InvalidOperationException($"Barcode '{dto.Barcode}' đã được dùng bởi sản phẩm khác.");

        product.Name = dto.Name;
        product.NameEn = dto.NameEn;
        product.Barcode = dto.Barcode;
        product.CategoryId = dto.CategoryId;
        product.UomId = dto.UomId;
        product.Description = dto.Description;
        product.MinStock = dto.MinStock;
        product.ReorderPoint = dto.ReorderPoint;
        product.IsBatchTracked = dto.IsBatchTracked;
        product.IsExpiryTracked = dto.IsExpiryTracked;
        product.ExpiryWarningDays = dto.ExpiryWarningDays;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();
        return product;
    }

    public async Task ToggleActivateAsync(int id)
    {
        var product = await _productRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID={id}.");
        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();
    }

    // ─── Category ─────────────────────────────────────────────────────────────
    public async Task<List<ProductCategory>> GetCategoryTreeAsync()
        => await _categoryRepo.GetTreeCategoriesAsync();

    public async Task<ProductCategory> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepo.ExistsAsync(c => c.Code.ToLower() == dto.Code.ToLower()))
            throw new InvalidOperationException($"Mã danh mục '{dto.Code}' đã tồn tại.");

        var category = new ProductCategory
        {
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            NameEn = dto.NameEn,
            ParentId = dto.ParentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();
        return category;
    }

    public async Task<ProductCategory> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy danh mục ID={id}.");
        category.Name = dto.Name;
        category.NameEn = dto.NameEn;
        category.ParentId = dto.ParentId;
        category.IsActive = dto.IsActive;
        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy danh mục ID={id}.");
        if (await _productRepo.ExistsAsync(p => p.CategoryId == id))
            throw new InvalidOperationException("Không thể xóa danh mục khi còn sản phẩm thuộc danh mục này.");
        if (await _categoryRepo.ExistsAsync(c => c.ParentId == id))
            throw new InvalidOperationException("Không thể xóa danh mục cha khi còn danh mục con.");

        _categoryRepo.Remove(category);
        await _categoryRepo.SaveChangesAsync();
    }

    // ─── UoM ──────────────────────────────────────────────────────────────────
    public async Task<List<UnitOfMeasure>> GetAllUoMAsync()
        => (await _uomRepo.GetAllAsync()).ToList();
}
