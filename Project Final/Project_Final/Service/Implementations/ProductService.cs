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

    public async Task<Product> CreateProductAsync(CreateProductDto dto, int? userId = null)
    {
        await ValidateProductAsync(dto.SKU, dto.Name, dto.Barcode, dto.CategoryId, dto.UomId, dto.MinStock, dto.ReorderPoint, dto.ExpiryWarningDays);

        var product = new Product
        {
            SKU = dto.SKU.Trim().ToUpper(),
            Name = dto.Name.Trim(),
            NameEn = dto.NameEn?.Trim(),
            Barcode = dto.Barcode?.Trim(),
            CategoryId = dto.CategoryId,
            UomId = dto.UomId,
            Description = dto.Description?.Trim(),
            MinStock = dto.MinStock,
            ReorderPoint = dto.ReorderPoint,
            IsBatchTracked = dto.IsBatchTracked,
            IsExpiryTracked = dto.IsExpiryTracked,
            ExpiryWarningDays = dto.ExpiryWarningDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId ?? 1
        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _productRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID={id}.");

        await ValidateProductAsync(product.SKU, dto.Name, dto.Barcode, dto.CategoryId, dto.UomId, dto.MinStock, dto.ReorderPoint, dto.ExpiryWarningDays, currentProductId: id);

        product.Name = dto.Name.Trim();
        product.NameEn = dto.NameEn?.Trim();
        product.Barcode = dto.Barcode?.Trim();
        product.CategoryId = dto.CategoryId;
        product.UomId = dto.UomId;
        product.Description = dto.Description?.Trim();
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

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID={id}.");

        var hasAssociatedRecords = await _productRepo.HasAssociatedRecordsAsync(id);
        if (hasAssociatedRecords)
        {
            throw new InvalidOperationException("Không thể xóa sản phẩm này vì đã có dữ liệu tồn kho, lô hàng hoặc chứng từ liên quan.");
        }

        _productRepo.Remove(product);
        await _productRepo.SaveChangesAsync();
    }

    private async Task ValidateProductAsync(
        string sku,
        string name,
        string? barcode,
        int categoryId,
        int uomId,
        decimal minStock,
        decimal reorderPoint,
        int expiryWarningDays,
        int? currentProductId = null)
    {
        // 1. SKU validation
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("Mã SKU sản phẩm là bắt buộc.");
        var trimmedSku = sku.Trim();
        if (trimmedSku.Length < 3 || trimmedSku.Length > 50)
            throw new ArgumentException("Mã SKU phải từ 3 đến 50 ký tự.");

        if (currentProductId == null)
        {
            if (await _productRepo.ExistsAsync(p => p.SKU.ToLower() == trimmedSku.ToLower()))
                throw new InvalidOperationException($"Mã SKU '{trimmedSku}' đã tồn tại.");
        }

        // 2. Name validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên sản phẩm là bắt buộc.");
        var trimmedName = name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 150)
            throw new ArgumentException("Tên sản phẩm phải từ 2 đến 150 ký tự.");

        // 3. Barcode validation (if provided)
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            var trimmedBarcode = barcode.Trim();
            if (currentProductId == null)
            {
                if (await _productRepo.ExistsAsync(p => p.Barcode != null && p.Barcode.ToLower() == trimmedBarcode.ToLower()))
                    throw new InvalidOperationException($"Mã Barcode '{trimmedBarcode}' đã tồn tại.");
            }
            else
            {
                if (await _productRepo.ExistsAsync(p => p.Id != currentProductId.Value && p.Barcode != null && p.Barcode.ToLower() == trimmedBarcode.ToLower()))
                    throw new InvalidOperationException($"Mã Barcode '{trimmedBarcode}' đã được sử dụng bởi sản phẩm khác.");
            }
        }

        // 4. Category & UoM validation
        if (!await _categoryRepo.ExistsAsync(c => c.Id == categoryId))
            throw new KeyNotFoundException($"Không tìm thấy danh mục ID={categoryId}.");

        if (!await _uomRepo.ExistsAsync(u => u.Id == uomId))
            throw new KeyNotFoundException($"Không tìm thấy đơn vị tính ID={uomId}.");

        // 5. Stock numbers validation
        if (minStock < 0)
            throw new ArgumentException("Mức tồn kho tối thiểu (MinStock) không được nhỏ hơn 0.");

        if (reorderPoint < 0)
            throw new ArgumentException("Điểm đặt hàng lại (ReorderPoint) không được nhỏ hơn 0.");

        if (expiryWarningDays < 0)
            throw new ArgumentException("Số ngày cảnh báo hết hạn không được nhỏ hơn 0.");
    }

    // ─── Category ─────────────────────────────────────────────────────────────
    public async Task<List<ProductCategory>> GetAllCategoriesAsync()
        => await _categoryRepo.GetAllWithParentAsync();

    public async Task<List<ProductCategory>> GetCategoryTreeAsync()
        => await _categoryRepo.GetTreeCategoriesAsync();

    public async Task<ProductCategory> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException("Mã danh mục là bắt buộc.");
        var trimmedCode = dto.Code.Trim();
        if (trimmedCode.Length < 2 || trimmedCode.Length > 50)
            throw new ArgumentException("Mã danh mục phải từ 2 đến 50 ký tự.");

        if (await _categoryRepo.ExistsAsync(c => c.Code.ToLower() == trimmedCode.ToLower()))
            throw new InvalidOperationException($"Mã danh mục '{trimmedCode}' đã tồn tại.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tên danh mục là bắt buộc.");
        var trimmedName = dto.Name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 100)
            throw new ArgumentException("Tên danh mục phải từ 2 đến 100 ký tự.");

        if (dto.ParentId.HasValue && !await _categoryRepo.ExistsAsync(c => c.Id == dto.ParentId.Value))
            throw new KeyNotFoundException($"Không tìm thấy danh mục cha ID={dto.ParentId.Value}.");

        var category = new ProductCategory
        {
            Code = trimmedCode.ToUpper(),
            Name = trimmedName,
            NameEn = dto.NameEn?.Trim(),
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

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tên danh mục là bắt buộc.");
        var trimmedName = dto.Name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 100)
            throw new ArgumentException("Tên danh mục phải từ 2 đến 100 ký tự.");

        if (dto.ParentId.HasValue)
        {
            if (dto.ParentId.Value == id)
                throw new ArgumentException("Danh mục cha không thể là chính danh mục này.");
            if (!await _categoryRepo.ExistsAsync(c => c.Id == dto.ParentId.Value))
                throw new KeyNotFoundException($"Không tìm thấy danh mục cha ID={dto.ParentId.Value}.");
        }

        category.Name = trimmedName;
        category.NameEn = dto.NameEn?.Trim();
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
