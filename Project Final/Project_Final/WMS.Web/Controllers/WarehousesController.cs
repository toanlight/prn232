using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class WarehousesController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public WarehousesController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? keyword, int pageIndex = 1, int pageSize = 10)
    {
        var endpoint = $"api/warehouses?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<WarehouseItemViewModel>>(endpoint);
        ViewBag.Keyword = keyword;
        ViewBag.PageIndex = pageIndex;
        ViewBag.PageSize = pageSize;

        return View(response ?? new PagedResponseModel<WarehouseItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest dto)
    {
        try
        {
            await _apiClient.PostAsync<object>("api/warehouses", dto);
            return Json(new { success = true, message = "Tạo kho hàng thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseRequest dto)
    {
        try
        {
            await _apiClient.PutAsync<object>($"api/warehouses/{id}", dto);
            return Json(new { success = true, message = "Cập nhật kho hàng thành công!" });
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
            await _apiClient.DeleteAsync($"api/warehouses/{id}");
            return Json(new { success = true, message = "Xóa kho hàng thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("Warehouses/{id}/Structure")]
    public async Task<IActionResult> Structure(int id)
    {
        var response = await _apiClient.GetAsync<ApiResponseModel<WarehouseItemViewModel>>($"api/warehouses/{id}");
        ViewBag.WarehouseId = id;
        return View(response?.Data ?? new WarehouseItemViewModel { Id = id, Code = $"WH-{id}", Name = "Kho hàng" });
    }
}

public record CreateWarehouseRequest(string Code, string Name, string? Address, int? ManagerUserId);
public record UpdateWarehouseRequest(string Name, string? Address, int? ManagerUserId, bool IsActive);
