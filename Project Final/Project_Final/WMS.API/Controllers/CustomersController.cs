using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] bool? isActive,
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 100)
    {
        var (items, totalCount) = await _customerService.SearchCustomersAsync(keyword, isActive, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(c => new {
                c.Id, c.Code, c.Name, CustomerType = c.CustomerType.ToString(), c.TaxCode, c.Address,
                c.Email, c.Phone, c.ContactPerson, c.ContactPhone, c.ContractNumber,
                c.ContractStartDate, c.ContractEndDate, c.ServiceTerms, c.Notes,
                c.IsActive, c.CreatedAt, c.UpdatedAt,
                CreatedBy = c.CreatedByUser != null ? (c.CreatedByUser.FullName ?? c.CreatedByUser.Username) : (c.CreatedBy.HasValue ? $"User #{c.CreatedBy}" : null),
                UpdatedBy = c.UpdatedByUser != null ? (c.UpdatedByUser.FullName ?? c.UpdatedByUser.Username) : (c.UpdatedBy.HasValue ? $"User #{c.UpdatedBy}" : null)
            }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(new {
            customer.Id, customer.Code, customer.Name, CustomerType = customer.CustomerType.ToString(),
            customer.TaxCode, customer.Address, customer.Email, customer.Phone, customer.ContactPerson,
            customer.ContactPhone, customer.ContractNumber, customer.ContractStartDate, customer.ContractEndDate,
            customer.ServiceTerms, customer.Notes, customer.IsActive, customer.CreatedAt, customer.UpdatedAt,
            CreatedBy = customer.CreatedByUser != null ? (customer.CreatedByUser.FullName ?? customer.CreatedByUser.Username) : (customer.CreatedBy.HasValue ? $"User #{customer.CreatedBy}" : null),
            UpdatedBy = customer.UpdatedByUser != null ? (customer.UpdatedByUser.FullName ?? customer.UpdatedByUser.Username) : (customer.UpdatedBy.HasValue ? $"User #{customer.UpdatedBy}" : null)
        }));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,SALES")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        var customer = await _customerService.CreateCustomerAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { customer.Id, customer.Code, customer.Name }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,SALES")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        var customer = await _customerService.UpdateCustomerAsync(id, dto, userId);
        return Ok(ApiResponse<object>.Ok(new { customer.Id, customer.Name }));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "ADMIN,SALES")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeCustomerStatusRequest request)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        await _customerService.ChangeStatusAsync(id, request.IsActive, userId);
        return Ok(ApiResponse.OkMessage("Trạng thái khách hàng đã được cập nhật."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,SALES")]
    public async Task<IActionResult> Delete(int id)
    {
        int? userId = null;
        try { userId = User.GetUserId(); } catch { }
        await _customerService.DeleteCustomerAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Xóa khách hàng thành công!"));
    }
}

public record ChangeCustomerStatusRequest(bool IsActive);
