using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/approvals")]
[Authorize(Roles = "WH_MANAGER,DIRECTOR")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalsController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var approverId = User.GetUserId();
        var approvals = await _approvalService.GetInboxAsync(approverId);
        return Ok(ApiResponse<object>.Ok(approvals));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var approverId = User.GetUserId();
        var approvals = await _approvalService.GetHistoryAsync(approverId);
        return Ok(ApiResponse<object>.Ok(approvals));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovalActionRequest? request)
    {
        var approverId = User.GetUserId();
        await _approvalService.ApproveAsync(id, approverId, request?.Comment);
        return Ok(ApiResponse.OkMessage("Approved"));
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalActionRequest request)
    {
        var approverId = User.GetUserId();
        await _approvalService.RejectAsync(id, approverId, request.Comment ?? "Rejected");
        return Ok(ApiResponse.OkMessage("Rejected"));
    }
}

public record ApprovalActionRequest(string? Comment);
