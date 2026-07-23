using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/transfer-orders")]
[Authorize(Roles = "WH_STAFF,WH_MANAGER,ADMIN")]
public class TransferOrdersController : ControllerBase
{
    private readonly ITransferOrderService _transferService;

    public TransferOrdersController(ITransferOrderService transferService)
    {
        _transferService = transferService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transfer = await _transferService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(transfer));
    }

    [HttpPost]
    [Authorize(Roles = "WH_STAFF,WH_MANAGER")]
    public async Task<IActionResult> Create([FromBody] CreateTransferDto dto)
    {
        var userId = User.GetUserId();
        var transfer = await _transferService.CreateAsync(dto, userId);
        return StatusCode(201, ApiResponse<object>.Created(new { transfer.Id, transfer.TransferNumber }));
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "WH_STAFF")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = User.GetUserId();
        await _transferService.SubmitAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Transfer order submitted for approval"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "WH_STAFF,WH_MANAGER")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.GetUserId();
        await _transferService.CancelAsync(id, userId);
        return Ok(ApiResponse.OkMessage("Transfer order cancelled"));
    }
}
