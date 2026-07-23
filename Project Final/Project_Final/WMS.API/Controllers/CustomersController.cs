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
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _customerService.SearchCustomersAsync(keyword, isActive, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(c => new { c.Id, c.Code, c.Name, c.TaxCode, c.Phone, c.Email, c.IsActive }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(customer));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,SALES")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var customer = await _customerService.CreateCustomerAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { customer.Id, customer.Code, customer.Name }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,SALES")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        var customer = await _customerService.UpdateCustomerAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { customer.Id, customer.Name }));
    }
}
