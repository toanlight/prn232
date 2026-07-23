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
        [FromQuery] bool? isActive, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _productService.SearchProductsAsync(keyword, categoryId, isActive, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(p => new { p.Id, p.SKU, p.Name, p.Barcode, CategoryName = p.Category?.Name, UomCode = p.Uom?.Code, p.MinStock, p.IsActive }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(product));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateProductAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { product.Id, product.SKU, product.Name }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateProductAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { product.Id, product.Name }));
    }

    [HttpPatch("{id}/toggle-activate")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ToggleActivate(int id)
    {
        await _productService.ToggleActivateAsync(id);
        return Ok(ApiResponse.OkMessage("Product activation toggled"));
    }

    // ──── Categories ────

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productService.GetCategoryTreeAsync();
        return Ok(ApiResponse<object>.Ok(categories));
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
