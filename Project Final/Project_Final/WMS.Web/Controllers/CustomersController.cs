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

    public async Task<IActionResult> Index(string? keyword, int pageIndex = 1)
    {
        var endpoint = $"api/customers?pageIndex={pageIndex}&pageSize=10";
        if (!string.IsNullOrEmpty(keyword))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<CustomerItemViewModel>>(endpoint);
        ViewBag.Keyword = keyword;
        return View(response ?? new PagedResponseModel<CustomerItemViewModel>());
    }
}
