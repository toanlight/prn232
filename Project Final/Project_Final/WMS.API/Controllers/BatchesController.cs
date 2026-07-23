using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Enums;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/batches")]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;

    public BatchesController(IBatchService batchService)
    {
        _batchService = batchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBatches([FromQuery] int? productId, [FromQuery] int? supplierId,
        [FromQuery] BatchStatus? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
    {
        var batches = await _batchService.GetBatchesAsync(productId, supplierId, status, pageIndex, pageSize);
        return Ok(ApiResponse<object>.Ok(batches));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var batch = await _batchService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(batch));
    }

    [HttpGet("expiring")]
    [Authorize(Roles = "WH_MANAGER,ADMIN")]
    public async Task<IActionResult> GetExpiring([FromQuery] int warningDays = 30)
    {
        var batches = await _batchService.GetExpiringBatchesAsync(warningDays);
        return Ok(ApiResponse<object>.Ok(batches));
    }

    [HttpGet("expired")]
    [Authorize(Roles = "WH_MANAGER,ADMIN")]
    public async Task<IActionResult> GetExpired()
    {
        var batches = await _batchService.GetExpiredBatchesAsync();
        return Ok(ApiResponse<object>.Ok(batches));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "WH_MANAGER,ADMIN")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBatchStatusRequest request)
    {
        await _batchService.UpdateBatchStatusAsync(id, request.Status);
        return Ok(ApiResponse.OkMessage("Batch status updated"));
    }
}

public record UpdateBatchStatusRequest(BatchStatus Status);
