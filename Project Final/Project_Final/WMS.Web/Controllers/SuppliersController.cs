using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public SuppliersController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Create()
    {
        return View("Form", new SupplierViewModel());
    }
}

public class SupplierViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
