using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // ──── Products ────

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] int? categoryId,
        [FromQuery] bool? isActive, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 100)
    {
        var (items, totalCount) = await _productService.SearchProductsAsync(keyword, categoryId, isActive, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(p => new {
                p.Id, p.SKU, p.Barcode, p.Name, p.NameEn,
                p.CategoryId, CategoryName = p.Category?.Name,
                p.UomId, UomCode = p.Uom?.Code, UomName = p.Uom?.Name,
                p.Description, p.MinStock, p.ReorderPoint,
                p.IsBatchTracked, p.IsExpiryTracked, p.ExpiryWarningDays,
                p.IsActive, p.CreatedAt, p.UpdatedAt,
                CreatedBy = p.CreatedByUser != null ? (p.CreatedByUser.FullName ?? p.CreatedByUser.Username) : (p.CreatedBy > 0 ? $"User #{p.CreatedBy}" : null)
            }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(new {
            product.Id, product.SKU, product.Barcode, product.Name, product.NameEn,
            product.CategoryId, CategoryName = product.Category?.Name,
            product.UomId, UomCode = product.Uom?.Code, UomName = product.Uom?.Name,
            product.Description, product.MinStock, product.ReorderPoint,
            product.IsBatchTracked, product.IsExpiryTracked, product.ExpiryWarningDays,
            product.IsActive, product.CreatedAt, product.UpdatedAt,
            CreatedBy = product.CreatedByUser != null ? (product.CreatedByUser.FullName ?? product.CreatedByUser.Username) : (product.CreatedBy > 0 ? $"User #{product.CreatedBy}" : null)
        }));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        var product = await _productService.CreateProductAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { product.Id, product.SKU, product.Name }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateProductAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { product.Id, product.Name }));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        return Ok(ApiResponse.OkMessage("Xóa sản phẩm thành công!"));
    }

    // ──── Categories ────

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productService.GetAllCategoriesAsync();
        return Ok(ApiResponse<object>.Ok(categories.Select(c => new {
            c.Id, c.Code, c.Name, c.NameEn, c.ParentId, ParentName = c.Parent?.Name, c.IsActive, c.CreatedAt
        })));
    }

    [HttpPost("categories")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var category = await _productService.CreateCategoryAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { category.Id, category.Code, category.Name }));
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _productService.UpdateCategoryAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { category.Id, category.Name }));
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _productService.DeleteCategoryAsync(id);
        return Ok(ApiResponse.OkMessage("Category deleted"));
    }

    // ──── UoM ────

    [HttpGet("uom")]
    public async Task<IActionResult> GetUoMs()
    {
        var uoms = await _productService.GetAllUoMAsync();
        return Ok(ApiResponse<object>.Ok(uoms));
    }
}
