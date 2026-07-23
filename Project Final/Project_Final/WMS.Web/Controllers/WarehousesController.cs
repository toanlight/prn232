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

    public async Task<IActionResult> Index()
    {
        var response = await _apiClient.GetAsync<PagedResponseModel<WarehouseItemViewModel>>("api/warehouses");
        return View(response ?? new PagedResponseModel<WarehouseItemViewModel>());
    }

    [HttpGet("Warehouses/{id}/Structure")]
    public async Task<IActionResult> Structure(int id)
    {
        ViewBag.WarehouseId = id;
        return View();
    }
}
