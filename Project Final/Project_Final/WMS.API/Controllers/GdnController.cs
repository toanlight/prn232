using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/gdn")]
[Authorize]
public class GdnController : ControllerBase
{
    private readonly IGdnService _gdnService;

    public GdnController(IGdnService gdnService)
    {
        _gdnService = gdnService;
    }

    [HttpGet]
    [Authorize(Roles = "WH_STAFF,WH_MANAGER,ADMIN,SALES")]
    public async Task<IActionResult> Search([FromQuery] string? status, [FromQuery] int? customerId,
        [FromQuery] int? warehouseId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _gdnService.SearchAsync(status, customerId, warehouseId, fromDate, toDate, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(g => new { g.Id, g.GDNNumber, g.Status, g.DispatchDate, CustomerName = g.Customer?.Name }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gdn = await _gdnService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(gdn));
    }

    [HttpPost]
    [Authorize(Roles = "WH_STAFF,SALES")]
    public async Task<IActionResult> Create([FromBody] CreateGdnDto dto)
    {
        var userId = User.GetUserId();
        var gdn = await _gdnService.CreateAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { gdn.Id, gdn.GDNNumber }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "WH_STAFF,SALES")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGdnDto dto)
    {
        var userId = User.GetUserId();
        var gdn = await _gdnService.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(new { gdn.Id, gdn.GDNNumber }));
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = User.GetUserId();
        await _gdnService.SubmitAsync(id, userId);
        return Ok(ApiResponse.OkMessage("GDN submitted for approval"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "WH_STAFF,WH_MANAGER")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.GetUserId();
        await _gdnService.CancelAsync(id, userId);
        return Ok(ApiResponse.OkMessage("GDN cancelled"));
    }

    [HttpPost("{id}/start-picking")]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> StartPicking(int id)
    {
        var userId = User.GetUserId();
        await _gdnService.StartPickingAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Picking started"));
    }

    [HttpPost("{id}/confirm-picked")]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> ConfirmPicked(int id)
    {
        var userId = User.GetUserId();
        await _gdnService.ConfirmPickedAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Picking confirmed"));
    }

    [HttpPost("{id}/deliver")]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> Deliver(int id, [FromBody] DeliverGdnDto dto)
    {
        var userId = User.GetUserId();
        await _gdnService.DeliverAsync(id, userId, dto);
        return Ok(ApiResponse.OkMessage("Delivery confirmed"));
    }
}
