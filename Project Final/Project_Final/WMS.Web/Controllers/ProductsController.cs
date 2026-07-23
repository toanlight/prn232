using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public ProductsController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index([FromQuery] string? keyword, [FromQuery] int pageIndex = 1)
    {
        ViewBag.Keyword = keyword;
        return View();
    }

    public IActionResult Create()
    {
        return View("Form", new ProductFormViewModel());
    }

    public IActionResult Edit(int id)
    {
        var vm = new ProductFormViewModel
        {
            Id = id,
            SKU = "SKU-FOOD-001",
            Name = "Sữa tươi tiệt trùng 1L",
            MinStock = 50,
            ReorderPoint = 100,
            IsBatchTracked = true,
            IsExpiryTracked = true
        };
        return View("Form", vm);
    }

    public IActionResult Detail(int id)
    {
        ViewBag.ProductId = id;
        return View();
    }

    public IActionResult Categories()
    {
        return View();
    }
}

public class ProductFormViewModel
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Barcode { get; set; }
    public int CategoryId { get; set; } = 1;
    public int UomId { get; set; } = 1;
    public decimal MinStock { get; set; } = 10;
    public decimal ReorderPoint { get; set; } = 20;
    public bool IsBatchTracked { get; set; } = true;
    public bool IsExpiryTracked { get; set; } = true;
    public int ExpiryWarningDays { get; set; } = 30;
    public string? Description { get; set; }
}
