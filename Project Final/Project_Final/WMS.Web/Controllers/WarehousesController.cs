using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class WarehousesController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public WarehousesController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _apiClient.GetAsync<ApiResponseDto<List<WarehouseViewModel>>>("api/warehouses");
        var warehouses = response?.Data ?? new List<WarehouseViewModel>();
        return View(warehouses);
    }

    [HttpGet("Warehouses/{id}/Structure")]
    public async Task<IActionResult> Structure(int id)
    {
        var response = await _apiClient.GetAsync<ApiResponseDto<WarehouseViewModel>>($"api/warehouses/{id}/hierarchy");
        var warehouse = response?.Data ?? new WarehouseViewModel { Id = id, Code = "WH-01", Name = "Kho Tổng Trung Tâm" };
        return View(warehouse);
    }

    [HttpGet("Warehouses/{id}/Zones")]
    public async Task<IActionResult> Zones(int id)
    {
        var response = await _apiClient.GetAsync<ApiResponseDto<List<ZoneViewModel>>>($"api/warehouses/{id}/zones");
        var zones = response?.Data ?? new List<ZoneViewModel>();
        ViewBag.WarehouseId = id;
        return View(zones);
    }

    [HttpGet("Zones/{id}/Locations")]
    public async Task<IActionResult> Locations(int id)
    {
        ViewBag.ZoneId = id;
        return View();
    }
}

public class WarehouseViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ZoneViewModel> Zones { get; set; } = new();
}

public class ZoneViewModel
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ZoneType { get; set; } = "STORAGE";
    public bool IsActive { get; set; } = true;
}
