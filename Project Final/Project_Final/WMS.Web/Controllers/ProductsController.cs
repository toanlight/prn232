using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public ProductsController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? keyword, int? categoryId, int pageIndex = 1, int pageSize = 100)
    {
        var endpoint = $"api/products?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
        }
        if (categoryId.HasValue)
        {
            endpoint += $"&categoryId={categoryId.Value}";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<ProductItemViewModel>>(endpoint);

        var categoriesRes = await _apiClient.GetAsync<ApiResponseModel<List<CategoryItemViewModel>>>("api/products/categories");
        var uomsRes = await _apiClient.GetAsync<ApiResponseModel<List<UomItemViewModel>>>("api/products/uom");

        ViewBag.Keyword = keyword;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = categoriesRes?.Data ?? new List<CategoryItemViewModel>();
        ViewBag.Uoms = uomsRes?.Data ?? new List<UomItemViewModel>();
        ViewBag.PageIndex = pageIndex;
        ViewBag.PageSize = pageSize;

        return View(response ?? new PagedResponseModel<ProductItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest dto)
    {
        try
        {
            await _apiClient.PostAsync<object>("api/products", dto);
            return Json(new { success = true, message = "Tạo sản phẩm thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest dto)
    {
        try
        {
            await _apiClient.PutAsync<object>($"api/products/{id}", dto);
            return Json(new { success = true, message = "Cập nhật sản phẩm thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _apiClient.DeleteAsync($"api/products/{id}");
            return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> Categories()
    {
        var response = await _apiClient.GetAsync<ApiResponseModel<List<CategoryItemViewModel>>>("api/products/categories");
        return View(response?.Data ?? new List<CategoryItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest dto)
    {
        try
        {
            await _apiClient.PostAsync<object>("api/products/categories", dto);
            return Json(new { success = true, message = "Tạo danh mục thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest dto)
    {
        try
        {
            await _apiClient.PutAsync<object>($"api/products/categories/{id}", dto);
            return Json(new { success = true, message = "Cập nhật danh mục thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await _apiClient.DeleteAsync($"api/products/categories/{id}");
            return Json(new { success = true, message = "Xóa danh mục thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}

public record CreateProductRequest(
    string SKU, string Name, string? NameEn, string? Barcode,
    int CategoryId, int UomId,
    decimal MinStock, decimal ReorderPoint,
    bool IsBatchTracked, bool IsExpiryTracked,
    int ExpiryWarningDays, string? Description
);

public record UpdateProductRequest(
    string Name, string? NameEn, string? Barcode,
    int CategoryId, int UomId,
    decimal MinStock, decimal ReorderPoint,
    bool IsBatchTracked, bool IsExpiryTracked,
    int ExpiryWarningDays, string? Description
);

public record CreateCategoryRequest(string Code, string Name, string? NameEn, int? ParentId);
public record UpdateCategoryRequest(string Name, string? NameEn, int? ParentId, bool IsActive);
