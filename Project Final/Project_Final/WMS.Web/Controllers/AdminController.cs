using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public AdminController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Users");
    }

    public IActionResult Users()
    {
        return View();
    }

    public IActionResult Settings()
    {
        return View();
    }

    public IActionResult Notifications()
    {
        return View();
    }
}
