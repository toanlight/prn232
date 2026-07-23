using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public DashboardController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var summary = await _apiClient.GetAsync<ApiResponseDto<DashboardSummaryDto>>("api/dashboard/summary");
            var lowStock = await _apiClient.GetAsync<ApiResponseDto<List<LowStockItemDto>>>("api/dashboard/low-stock?top=10");
            var expiringBatches = await _apiClient.GetAsync<ApiResponseDto<List<ExpiringBatchItemDto>>>("api/dashboard/expiring-batches?withinDays=30");
            var recentActivities = await _apiClient.GetAsync<ApiResponseDto<List<RecentActivityDto>>>("api/dashboard/recent-activities?limit=10");

            ViewBag.Summary = summary?.Data;
            ViewBag.LowStock = lowStock?.Data ?? new List<LowStockItemDto>();
            ViewBag.ExpiringBatches = expiringBatches?.Data ?? new List<ExpiringBatchItemDto>();
            ViewBag.RecentActivities = recentActivities?.Data ?? new List<RecentActivityDto>();
        }
        catch
        {
            // Fallback empty view values if API isn't running yet
            ViewBag.Summary = new DashboardSummaryDto(0, 0, 0, 0, 0);
            ViewBag.LowStock = new List<LowStockItemDto>();
            ViewBag.ExpiringBatches = new List<ExpiringBatchItemDto>();
            ViewBag.RecentActivities = new List<RecentActivityDto>();
        }

        return View();
    }
}

public record DashboardSummaryDto(
    int TotalActiveSkus,
    int TodayTransactions,
    int LowStockAlerts,
    int ExpiringBatchAlerts,
    int PendingApprovals
);

public record LowStockItemDto(
    int ProductId,
    string SKU,
    string Name,
    decimal TotalQty,
    decimal MinStock,
    decimal ShortfallQty
);

public record ExpiringBatchItemDto(
    int BatchId,
    string LotNumber,
    string ProductName,
    string SKU,
    string? ExpiryDate,
    int DaysToExpiry,
    decimal Quantity
);

public record RecentActivityDto(
    long TransactionId,
    string Type,
    string ProductSku,
    string ProductName,
    decimal Qty,
    string BinCode,
    string CreatedBy,
    DateTime CreatedAt
);
