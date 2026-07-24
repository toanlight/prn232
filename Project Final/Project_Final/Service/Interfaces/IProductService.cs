using BusinessLayer.Entities.Products;

namespace Service.Interfaces;

public interface IProductService
{
    Task<(List<Product> Items, int TotalCount)> SearchProductsAsync(string? keyword, int? categoryId, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<Product> GetByIdAsync(int id);
    Task<Product> CreateProductAsync(CreateProductDto dto, int? userId = null);
    Task<Product> UpdateProductAsync(int id, UpdateProductDto dto);
    Task DeleteProductAsync(int id);

    // Category
    Task<List<ProductCategory>> GetAllCategoriesAsync();
    Task<List<ProductCategory>> GetCategoryTreeAsync();
    Task<ProductCategory> CreateCategoryAsync(CreateCategoryDto dto);
    Task<ProductCategory> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(int id);

    // UoM
    Task<List<UnitOfMeasure>> GetAllUoMAsync();
}

public record CreateProductDto(
    string SKU, string Name, string? NameEn, string? Barcode,
    int CategoryId, int UomId,
    decimal MinStock, decimal ReorderPoint,
    bool IsBatchTracked, bool IsExpiryTracked,
    int ExpiryWarningDays, string? Description
);

public record UpdateProductDto(
    string Name, string? NameEn, string? Barcode,
    int CategoryId, int UomId,
    decimal MinStock, decimal ReorderPoint,
    bool IsBatchTracked, bool IsExpiryTracked,
    int ExpiryWarningDays, string? Description
);

public record CreateCategoryDto(string Code, string Name, string? NameEn, int? ParentId);
public record UpdateCategoryDto(string Name, string? NameEn, int? ParentId, bool IsActive);
