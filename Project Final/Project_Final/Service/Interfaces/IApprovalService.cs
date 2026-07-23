using BusinessLayer.Entities.Approvals;
using BusinessLayer.Enums;

namespace Service.Interfaces;

public interface IApprovalService
{
    Task<List<Approval>> GetInboxAsync(int approverId);
    Task<List<Approval>> GetHistoryAsync(int approverId);
    Task ApproveAsync(int approvalId, int approverId, string? comment);
    Task RejectAsync(int approvalId, int approverId, string comment);

    /// <summary>
    /// Tạo bản ghi Approval đầu tiên (L1) khi một chứng từ được Submit.
    /// Gọi từ GrnService, GdnService, TransferOrderService, StockAdjustmentService.
    /// </summary>
    Task CreateApprovalRequestAsync(DocumentType documentType, int documentId, int createdByUserId);
}
