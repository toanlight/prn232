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
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 100)
    {
        var (items, totalCount) = await _supplierService.SearchSuppliersAsync(keyword, isActive, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(s => new {
                s.Id, s.Code, s.Name, s.TaxCode, s.Address, s.Email, s.Phone,
                s.Website, s.ContactPerson, s.ContactPhone, s.ContractNumber,
                s.ContractStartDate, s.ContractEndDate, s.PaymentTerms, s.Notes,
                Status = s.Status.ToString(), s.IsActive, s.CreatedAt, s.UpdatedAt,
                CreatedBy = s.CreatedByUser != null ? (s.CreatedByUser.FullName ?? s.CreatedByUser.Username) : (s.CreatedBy.HasValue ? $"User #{s.CreatedBy}" : null),
                UpdatedBy = s.UpdatedByUser != null ? (s.UpdatedByUser.FullName ?? s.UpdatedByUser.Username) : (s.UpdatedBy.HasValue ? $"User #{s.UpdatedBy}" : null)
            }).ToList<object>(),
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
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        var supplier = await _supplierService.CreateSupplierAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { supplier.Id, supplier.Code, supplier.Name }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,PURCHASING")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        var supplier = await _supplierService.UpdateSupplierAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(new { supplier.Id, supplier.Name }));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        await _supplierService.ChangeStatusAsync(id, request.Status, userId);
        return Ok(ApiResponse.OkMessage("Status updated"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,PURCHASING")]
    public async Task<IActionResult> Delete(int id)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        await _supplierService.DeleteSupplierAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Xóa nhà cung cấp thành công!"));
    }
}

public record ChangeStatusRequest(SupplierStatus Status);
