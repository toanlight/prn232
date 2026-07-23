using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class ApprovalsController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public ApprovalsController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index([FromQuery] string? tab = "inbox")
    {
        ViewBag.Tab = tab;
        return View("Inbox");
    }

    public IActionResult Inbox()
    {
        return View();
    }
}
