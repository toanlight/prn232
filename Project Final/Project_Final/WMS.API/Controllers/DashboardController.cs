using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(ApiResponse<object>.Ok(summary));
    }

    [HttpGet("movement-chart")]
    public async Task<IActionResult> GetMovementChart([FromQuery] int days = 7)
    {
        var chart = await _dashboardService.GetMovementChartAsync(days);
        return Ok(ApiResponse<object>.Ok(chart));
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "WH_MANAGER,DIRECTOR")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var approverId = User.GetUserId();
        var pending = await _dashboardService.GetPendingApprovalsAsync(approverId);
        return Ok(ApiResponse<object>.Ok(pending));
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "WH_MANAGER,ADMIN")]
    public async Task<IActionResult> GetLowStock([FromQuery] int top = 10)
    {
        var items = await _dashboardService.GetLowStockAsync(top);
        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("expiring-batches")]
    [Authorize(Roles = "WH_MANAGER,ADMIN")]
    public async Task<IActionResult> GetExpiringBatches([FromQuery] int withinDays = 30)
    {
        var batches = await _dashboardService.GetExpiringBatchesAsync(withinDays);
        return Ok(ApiResponse<object>.Ok(batches));
    }

    [HttpGet("recent-activities")]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 10)
    {
        var activities = await _dashboardService.GetRecentActivitiesAsync(limit);
        return Ok(ApiResponse<object>.Ok(activities));
    }
}
