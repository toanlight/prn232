using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/grn")]
[Authorize(Roles = "WH_STAFF,WH_MANAGER,ADMIN")]
public class GrnController : ControllerBase
{
    private readonly IGrnService _grnService;

    public GrnController(IGrnService grnService)
    {
        _grnService = grnService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? status, [FromQuery] int? supplierId,
        [FromQuery] int? warehouseId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _grnService.SearchAsync(status, supplierId, warehouseId, fromDate, toDate, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(g => new { g.Id, g.GRNNumber, g.Status, g.ReceiptDate, WarehouseName = g.Warehouse?.Name }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var grn = await _grnService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(grn));
    }

    [HttpPost]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> Create([FromBody] CreateGrnDto dto)
    {
        var userId = User.GetUserId();
        var grn = await _grnService.CreateAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { grn.Id, grn.GRNNumber }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGrnDto dto)
    {
        var userId = User.GetUserId();
        var grn = await _grnService.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(new { grn.Id, grn.GRNNumber }));
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = User.GetUserId();
        await _grnService.SubmitAsync(id, userId);
        return Ok(ApiResponse.OkMessage("GRN submitted for approval"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "WH_STAFF,WH_MANAGER")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.GetUserId();
        await _grnService.CancelAsync(id, userId);
        return Ok(ApiResponse.OkMessage("GRN cancelled"));
    }
}
