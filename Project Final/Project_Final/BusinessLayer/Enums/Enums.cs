namespace BusinessLayer.Enums;

/// <summary>
/// Trạng thái chung cho các chứng từ (GRN, GDN, Transfer, Adjustment...)
/// </summary>
public enum DocumentStatus
{
    Draft = 0,
    PendingL1 = 1,
    PendingL2 = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    // Trạng thái mở rộng cho GDN
    Picking = 6,
    Picked = 7,
    Delivered = 8,
    // Trạng thái mở rộng cho Transfer
    Completed = 9
}

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum ApprovalLevel
{
    L1Manager = 1,
    L2Director = 2
}

public enum CustomerType
{
    B2BService = 0,
    Consignee = 1
}

public enum SupplierStatus
{
    Active = 0,
    Inactive = 1,
    Blacklisted = 2
}

public enum BatchStatus
{
    Active = 0,
    Expired = 1,
    Consumed = 2,
    Recalled = 3
}

public enum CountType
{
    Full = 0,
    Periodic = 1,
    Cycle = 2
}

public enum StockTxnType
{
    GrnIn,
    GrnReturnOut,
    GdnOut,
    GdnReturnIn,
    TransferOut,
    TransferIn,
    AdjIn,
    AdjOut
}

public enum DocumentType
{
    Grn,
    Gdn,
    Transfer,
    StockAdjustment
}
