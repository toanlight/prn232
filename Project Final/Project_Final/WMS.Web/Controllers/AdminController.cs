using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public AdminController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Users");
    }

    public async Task<IActionResult> Users(string? keyword, int pageIndex = 1)
    {
        var endpoint = $"api/users?pageIndex={pageIndex}&pageSize=10";
        if (!string.IsNullOrEmpty(keyword))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<UserItemViewModel>>(endpoint);
        ViewBag.Keyword = keyword;
        return View(response ?? new PagedResponseModel<UserItemViewModel>());
    }
}
