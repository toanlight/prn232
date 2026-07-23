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

    public async Task<IActionResult> Index(string? keyword, int pageIndex = 1)
    {
        var endpoint = $"api/products?pageIndex={pageIndex}&pageSize=10";
        if (!string.IsNullOrEmpty(keyword))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<ProductItemViewModel>>(endpoint);
        ViewBag.Keyword = keyword;
        return View(response ?? new PagedResponseModel<ProductItemViewModel>());
    }

    public IActionResult Categories()
    {
        return View();
    }
}
