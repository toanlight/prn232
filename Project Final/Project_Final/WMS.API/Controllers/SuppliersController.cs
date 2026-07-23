using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Enums;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] bool? isActive,
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _supplierService.SearchSuppliersAsync(keyword, isActive, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(s => new { s.Id, s.Code, s.Name, s.TaxCode, s.Phone, s.Email, s.IsActive }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(supplier));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,PURCHASING")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        var supplier = await _supplierService.CreateSupplierAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { supplier.Id, supplier.Code, supplier.Name }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,PURCHASING")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        var supplier = await _supplierService.UpdateSupplierAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { supplier.Id, supplier.Name }));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
    {
        await _supplierService.ChangeStatusAsync(id, request.Status);
        return Ok(ApiResponse.OkMessage("Status updated"));
    }
}

public record ChangeStatusRequest(SupplierStatus Status);
