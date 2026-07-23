using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/stock-counts")]
[Authorize(Roles = "WH_STAFF,WH_MANAGER")]
public class StockCountsController : ControllerBase
{
    private readonly IStockCountService _stockCountService;

    public StockCountsController(IStockCountService stockCountService)
    {
        _stockCountService = stockCountService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var count = await _stockCountService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(count));
    }

    [HttpPost]
    [Authorize(Roles = "WH_MANAGER")]
    public async Task<IActionResult> Create([FromBody] CreateStockCountDto dto)
    {
        var userId = User.GetUserId();
        var count = await _stockCountService.CreateAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { count.Id, count.CountNumber }));
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(int id)
    {
        var userId = User.GetUserId();
        await _stockCountService.StartCountAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Count started"));
    }

    [HttpPatch("{countId}/lines/{lineId}")]
    public async Task<IActionResult> UpdateLineActualQty(int countId, int lineId, [FromBody] UpdateActualQtyRequest request)
    {
        var userId = User.GetUserId();
        await _stockCountService.UpdateLineActualQtyAsync(countId, lineId, request.ActualQty, userId);
        return Ok(ApiResponse.OkMessage("Actual quantity updated"));
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "WH_MANAGER")]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = User.GetUserId();
        await _stockCountService.CompleteCountAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Count completed"));
    }

    [HttpGet("{id}/variances")]
    [Authorize(Roles = "WH_MANAGER")]
    public async Task<IActionResult> GetVariances(int id)
    {
        var variances = await _stockCountService.GetVariancesAsync(id);
        return Ok(ApiResponse<object>.Ok(variances));
    }
}

public record UpdateActualQtyRequest(decimal ActualQty);
