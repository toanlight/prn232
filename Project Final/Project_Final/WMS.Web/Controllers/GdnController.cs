using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class GdnController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public GdnController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult Detail(int id)
    {
        ViewBag.GdnId = id;
        return View();
    }

    public IActionResult Picking(int id)
    {
        ViewBag.GdnId = id;
        return View();
    }

    public IActionResult Delivery(int id)
    {
        ViewBag.GdnId = id;
        return View();
    }
}
