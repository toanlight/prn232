namespace Service.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<List<MovementChartDto>> GetMovementChartAsync(int days = 7);
    Task<PendingApprovalsDto> GetPendingApprovalsAsync(int approverId);
    Task<List<LowStockItemDto>> GetLowStockAsync(int top = 10);
    Task<List<ExpiringBatchItemDto>> GetExpiringBatchesAsync(int withinDays = 30);
    Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 10);
}

public record DashboardSummaryDto(
    int TotalActiveSkus,
    int TodayTransactions,
    int LowStockAlerts,
    int ExpiringBatchAlerts,
    int PendingApprovals
);

public record MovementChartDto(
    DateOnly Date,
    decimal TotalIn,
    decimal TotalOut
);

public record PendingApprovalsDto(
    int Count,
    List<PendingApprovalItemDto> RecentItems
);

public record PendingApprovalItemDto(
    int ApprovalId,
    string DocumentType,
    int DocumentId,
    string DocumentNumber,
    string CreatedBy,
    DateTime SubmittedAt
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
    DateOnly? ExpiryDate,
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
