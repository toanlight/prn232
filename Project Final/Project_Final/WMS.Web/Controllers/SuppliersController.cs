using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public SuppliersController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? keyword, int pageIndex = 1, int pageSize = 100)
    {
        var endpoint = $"api/suppliers?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<SupplierItemViewModel>>(endpoint);
        ViewBag.Keyword = keyword;
        ViewBag.PageIndex = pageIndex;
        ViewBag.PageSize = pageSize;
        return View(response ?? new PagedResponseModel<SupplierItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest dto)
    {
        try
        {
            await _apiClient.PostAsync<object>("api/suppliers", dto);
            return Json(new { success = true, message = "Tạo nhà cung cấp thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest dto)
    {
        try
        {
            await _apiClient.PutAsync<object>($"api/suppliers/{id}", dto);
            return Json(new { success = true, message = "Cập nhật nhà cung cấp thành công!" });
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
            await _apiClient.DeleteAsync($"api/suppliers/{id}");
            return Json(new { success = true, message = "Xóa nhà cung cấp thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}

public record CreateSupplierRequest(
    string Code, string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? Website, string? ContactPerson,
    string? ContactPhone, string? ContractNumber, DateTime? ContractStartDate,
    DateTime? ContractEndDate, string? PaymentTerms, string? Notes
);

public record UpdateSupplierRequest(
    string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? Website, string? ContactPerson,
    string? ContactPhone, string? ContractNumber, DateTime? ContractStartDate,
    DateTime? ContractEndDate, string? PaymentTerms, string? Notes,
    int Status = 0
);
