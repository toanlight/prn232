using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public CustomersController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? keyword, int pageIndex = 1, int pageSize = 100)
    {
        var endpoint = $"api/customers?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<CustomerItemViewModel>>(endpoint);
        ViewBag.Keyword = keyword;
        ViewBag.PageIndex = pageIndex;
        ViewBag.PageSize = pageSize;
        return View(response ?? new PagedResponseModel<CustomerItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest dto)
    {
        try
        {
            await _apiClient.PostAsync<object>("api/customers", dto);
            return Json(new { success = true, message = "Tạo khách hàng thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest dto)
    {
        try
        {
            await _apiClient.PutAsync<object>($"api/customers/{id}", dto);
            return Json(new { success = true, message = "Cập nhật khách hàng thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeCustomerStatusRequest dto)
    {
        try
        {
            await _apiClient.PatchAsync<object>($"api/customers/{id}/status", dto);
            return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
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
            await _apiClient.DeleteAsync($"api/customers/{id}");
            return Json(new { success = true, message = "Xóa khách hàng thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}

public record CreateCustomerRequest(
    string Code, string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson, string? ContactPhone,
    string? ContractNumber, DateTime? ContractStartDate, DateTime? ContractEndDate,
    string? ServiceTerms, string? Notes, string CustomerType
);

public record UpdateCustomerRequest(
    string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson, string? ContactPhone,
    string? ContractNumber, DateTime? ContractStartDate, DateTime? ContractEndDate,
    string? ServiceTerms, string? Notes, string CustomerType, bool IsActive
);

public record ChangeCustomerStatusRequest(bool IsActive);
