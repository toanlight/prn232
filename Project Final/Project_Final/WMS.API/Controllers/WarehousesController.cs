using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    // ──── Warehouse ────

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var warehouses = await _warehouseService.GetActiveWarehousesAsync();
        return Ok(ApiResponse<object>.Ok(warehouses));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(warehouse));
    }

    [HttpGet("{id}/hierarchy")]
    public async Task<IActionResult> GetHierarchy(int id)
    {
        var warehouse = await _warehouseService.GetWithHierarchyAsync(id);
        return Ok(ApiResponse<object>.Ok(warehouse));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {
        var warehouse = await _warehouseService.CreateWarehouseAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { warehouse.Id, warehouse.Code, warehouse.Name }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseDto dto)
    {
        var warehouse = await _warehouseService.UpdateWarehouseAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { warehouse.Id, warehouse.Name }));
    }

    // ──── Zones ────

    [HttpGet("{warehouseId}/zones")]
    public async Task<IActionResult> GetZones(int warehouseId)
    {
        var zones = await _warehouseService.GetZonesByWarehouseAsync(warehouseId);
        return Ok(ApiResponse<object>.Ok(zones));
    }

    [HttpPost("zones")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> CreateZone([FromBody] CreateZoneDto dto)
    {
        var zone = await _warehouseService.CreateZoneAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { zone.Id, zone.Code, zone.Name }));
    }

    [HttpPut("zones/{id}")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> UpdateZone(int id, [FromBody] UpdateZoneDto dto)
    {
        var zone = await _warehouseService.UpdateZoneAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { zone.Id, zone.Name }));
    }

    [HttpDelete("zones/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteZone(int id)
    {
        await _warehouseService.DeleteZoneAsync(id);
        return Ok(ApiResponse.OkMessage("Zone deleted"));
    }

    // ──── Racks ────

    [HttpGet("zones/{zoneId}/racks")]
    public async Task<IActionResult> GetRacks(int zoneId)
    {
        var racks = await _warehouseService.GetRacksByZoneAsync(zoneId);
        return Ok(ApiResponse<object>.Ok(racks));
    }

    [HttpPost("racks")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> CreateRack([FromBody] CreateRackDto dto)
    {
        var rack = await _warehouseService.CreateRackAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { rack.Id, rack.Code, rack.Name }));
    }

    [HttpPut("racks/{id}")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> UpdateRack(int id, [FromBody] UpdateRackDto dto)
    {
        var rack = await _warehouseService.UpdateRackAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { rack.Id, rack.Name }));
    }

    // ──── Shelves ────

    [HttpGet("racks/{rackId}/shelves")]
    public async Task<IActionResult> GetShelves(int rackId)
    {
        var shelves = await _warehouseService.GetShelvesByRackAsync(rackId);
        return Ok(ApiResponse<object>.Ok(shelves));
    }

    [HttpPost("shelves")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> CreateShelf([FromBody] CreateShelfDto dto)
    {
        var shelf = await _warehouseService.CreateShelfAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { shelf.Id, shelf.Code, shelf.Name }));
    }

    [HttpPut("shelves/{id}")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> UpdateShelf(int id, [FromBody] UpdateShelfDto dto)
    {
        var shelf = await _warehouseService.UpdateShelfAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { shelf.Id, shelf.Name }));
    }

    // ──── Bins ────

    [HttpGet("shelves/{shelfId}/bins")]
    public async Task<IActionResult> GetBins(int shelfId)
    {
        var bins = await _warehouseService.GetBinsByShelfAsync(shelfId);
        return Ok(ApiResponse<object>.Ok(bins));
    }

    [HttpPost("bins")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> CreateBin([FromBody] CreateBinDto dto)
    {
        var bin = await _warehouseService.CreateBinAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { bin.Id, bin.Code, bin.Name }));
    }

    [HttpPut("bins/{id}")]
    [Authorize(Roles = "ADMIN,WH_MANAGER")]
    public async Task<IActionResult> UpdateBin(int id, [FromBody] UpdateBinDto dto)
    {
        var bin = await _warehouseService.UpdateBinAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { bin.Id, bin.Name }));
    }
}
