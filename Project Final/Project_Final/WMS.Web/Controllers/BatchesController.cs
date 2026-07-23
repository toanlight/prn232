using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public IActionResult Index([FromQuery] string? tab = "all")
    {
        ViewBag.Tab = tab;
        return View();
    }

    public IActionResult Detail(int id)
    {
        ViewBag.BatchId = id;
        return View();
    }

    public IActionResult Expiring()
    {
        return View();
    }
}
