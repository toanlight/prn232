using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public ReportsController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return View();
    }
}
