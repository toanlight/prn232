using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class StockController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public StockController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Current");
    }

    public IActionResult Current()
    {
        return View();
    }

    public IActionResult ByLocation()
    {
        return View();
    }

    public IActionResult ByBatch()
    {
        return View();
    }
}
