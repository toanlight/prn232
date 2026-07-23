using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class StockCountsController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public StockCountsController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Execution(int id)
    {
        ViewBag.CountId = id;
        return View();
    }

    public IActionResult Result(int id)
    {
        ViewBag.CountId = id;
        return View();
    }
}
