using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class PurchaseOrdersController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public PurchaseOrdersController(IWmsApiClient apiClient)
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
        ViewBag.PoId = id;
        return View();
    }
}
