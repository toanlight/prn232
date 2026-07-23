using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class BatchesController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public BatchesController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? tab = "all", int pageIndex = 1)
    {
        var endpoint = $"api/batches?pageIndex={pageIndex}&pageSize=10";
        if (tab == "expiring")
        {
            endpoint += "&status=ExpiringSoon";
        }
        else if (tab == "expired")
        {
            endpoint += "&status=Expired";
        }

        var response = await _apiClient.GetAsync<PagedResponseModel<BatchItemViewModel>>(endpoint);
        ViewBag.Tab = tab;
        return View(response ?? new PagedResponseModel<BatchItemViewModel>());
    }
}
