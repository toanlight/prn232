using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/stock-adjustments")]
[Authorize(Roles = "WH_MANAGER,ADMIN")]
public class StockAdjustmentsController : ControllerBase
{
    private readonly IStockAdjustmentService _adjustmentService;

    public StockAdjustmentsController(IStockAdjustmentService adjustmentService)
    {
        _adjustmentService = adjustmentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var adjustment = await _adjustmentService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(adjustment));
    }

    [HttpPost]
    [Authorize(Roles = "WH_MANAGER")]
    public async Task<IActionResult> Create([FromBody] CreateAdjustmentDto dto)
    {
        var userId = User.GetUserId();
        var adjustment = await _adjustmentService.CreateAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { adjustment.Id, adjustment.AdjNumber }));
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "WH_MANAGER")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = User.GetUserId();
        await _adjustmentService.SubmitAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Stock adjustment submitted for approval"));
    }
}
