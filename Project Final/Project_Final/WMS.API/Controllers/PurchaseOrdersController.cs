using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize(Roles = "PURCHASING,WH_MANAGER,ADMIN")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _poService;

    public PurchaseOrdersController(IPurchaseOrderService poService)
    {
        _poService = poService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? poNumber, [FromQuery] int? supplierId,
        [FromQuery] int? warehouseId, [FromQuery] string? status,
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _poService.SearchAsync(poNumber, supplierId, warehouseId, status, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(po => new { po.Id, po.PONumber, po.Status, po.OrderDate, SupplierName = po.Supplier?.Name, WarehouseName = po.Warehouse?.Name }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var po = await _poService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(po));
    }

    [HttpPost]
    [Authorize(Roles = "PURCHASING")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var userId = User.GetUserId();
        var po = await _poService.CreateAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { po.Id, po.PONumber }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "PURCHASING")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderDto dto)
    {
        var userId = User.GetUserId();
        var po = await _poService.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(new { po.Id, po.PONumber }));
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "PURCHASING")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = User.GetUserId();
        await _poService.SubmitAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Purchase order submitted"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "PURCHASING")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.GetUserId();
        await _poService.CancelAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Purchase order cancelled"));
    }
}
