using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class GrnController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public GrnController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index([FromQuery] string? tab = "all")
    {
        ViewBag.Tab = tab;
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult Detail(int id)
    {
        ViewBag.GrnId = id;
        return View();
    }
}
