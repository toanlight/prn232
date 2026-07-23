using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/stock")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentStock([FromQuery] int? warehouseId, [FromQuery] int? categoryId, [FromQuery] bool? belowMinStock)
    {
        var stock = await _stockService.GetCurrentStockAsync(warehouseId, categoryId, belowMinStock);
        return Ok(ApiResponse<object>.Ok(stock));
    }

    [HttpGet("by-location")]
    public async Task<IActionResult> GetByLocation([FromQuery] int? warehouseId, [FromQuery] int? zoneId, [FromQuery] int? binId)
    {
        var stock = await _stockService.GetStockByLocationAsync(warehouseId, zoneId, binId);
        return Ok(ApiResponse<object>.Ok(stock));
    }

    [HttpGet("by-batch")]
    public async Task<IActionResult> GetByBatch([FromQuery] int? productId)
    {
        var stock = await _stockService.GetStockByBatchAsync(productId);
        return Ok(ApiResponse<object>.Ok(stock));
    }

    [HttpGet("fefo-suggestion")]
    [Authorize(Roles = "WH_STAFF,WH_MANAGER")]
    public async Task<IActionResult> GetFefoSuggestion([FromQuery] int productId, [FromQuery] decimal quantity, [FromQuery] int warehouseId)
    {
        var suggestion = await _stockService.GetFefoSuggestionAsync(productId, quantity, warehouseId);
        return Ok(ApiResponse<object>.Ok(suggestion));
    }

    [HttpGet("available/{productId}")]
    public async Task<IActionResult> GetAvailable(int productId)
    {
        var qty = await _stockService.GetAvailableStockAsync(productId);
        return Ok(ApiResponse<object>.Ok(new { ProductId = productId, AvailableQty = qty }));
    }
}
