using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "WH_MANAGER,ADMIN")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetCurrentStockReport([FromQuery] int? warehouseId, [FromQuery] int? categoryId, [FromQuery] bool? belowMinStock)
    {
        var report = await _reportService.GetCurrentStockReportAsync(warehouseId, categoryId, belowMinStock);
        return Ok(ApiResponse<object>.Ok(report));
    }

    [HttpGet("inventory-movement")]
    public async Task<IActionResult> GetInventoryMovementReport([FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int? warehouseId, [FromQuery] int? productId, [FromQuery] string? txnType)
    {
        var report = await _reportService.GetInventoryMovementReportAsync(from, to, warehouseId, productId, txnType);
        return Ok(ApiResponse<object>.Ok(report));
    }

    [HttpGet("expiry")]
    public async Task<IActionResult> GetExpiryReport([FromQuery] int? warehouseId, [FromQuery] int warningDays = 30)
    {
        var report = await _reportService.GetExpiryReportAsync(warehouseId, warningDays);
        return Ok(ApiResponse<object>.Ok(report));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportReport([FromQuery] string reportType, [FromQuery] string format = "xlsx")
    {
        var parameters = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        var fileBytes = await _reportService.ExportReportAsync(reportType, format, parameters);
        
        string contentType = format.ToLower() switch
        {
            "pdf" => "application/pdf",
            "csv" => "text/csv",
            _ => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
        string fileName = $"{reportType}_Report_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";
        
        return File(fileBytes, contentType, fileName);
    }
}
