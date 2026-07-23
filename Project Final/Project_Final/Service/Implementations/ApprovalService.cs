using BusinessLayer.Entities.Approvals;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _approvalRepo;
    private readonly INotificationService _notifRepo;
    private readonly IServiceProvider _serviceProvider;

    public ApprovalService(
        IApprovalRepository approvalRepo,
        INotificationService notifRepo,
        IServiceProvider serviceProvider)
    {
        _approvalRepo = approvalRepo;
        _notifRepo = notifRepo;
        _serviceProvider = serviceProvider;
    }

    public async Task CreateApprovalRequestAsync(DocumentType documentType, int documentId, int createdByUserId)
    {
        var approval = new Approval
        {
            DocumentType = documentType,
            DocumentId = documentId,
            Level = ApprovalLevel.L1Manager,
            Status = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _approvalRepo.AddAsync(approval);
        await _approvalRepo.SaveChangesAsync();
    }

    public async Task<List<Approval>> GetInboxAsync(int approverId)
    {
        return await _approvalRepo.GetQueryable()
            .Include(a => a.Approver)
            .Where(a => a.Status == ApprovalStatus.Pending)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Approval>> GetHistoryAsync(int approverId)
    {
        return await _approvalRepo.GetQueryable()
            .Include(a => a.Approver)
            .Where(a => a.ApproverId == approverId && a.Status != ApprovalStatus.Pending)
            .OrderByDescending(a => a.ApprovedAt)
            .ToListAsync();
    }

    public async Task ApproveAsync(int approvalId, int approverId, string? comment)
    {
        var approval = await _approvalRepo.GetByIdAsync(approvalId)
            ?? throw new KeyNotFoundException($"Không tìm thấy phiếu trình duyệt ID={approvalId}.");

        if (approval.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Phiếu này đã được xử lý trước đó.");

        approval.ApproverId = approverId;
        approval.Comment = comment;
        approval.ApprovedAt = DateTime.UtcNow;
        approval.Status = ApprovalStatus.Approved;
        _approvalRepo.Update(approval);
        await _approvalRepo.SaveChangesAsync();

        if (approval.Level == ApprovalLevel.L1Manager)
        {
            // L1 Approve -> Tạo L2 Approval
            var l2Approval = new Approval
            {
                DocumentType = approval.DocumentType,
                DocumentId = approval.DocumentId,
                Level = ApprovalLevel.L2Director,
                Status = ApprovalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _approvalRepo.AddAsync(l2Approval);
            await _approvalRepo.SaveChangesAsync();
        }
        else if (approval.Level == ApprovalLevel.L2Director)
        {
            // L2 Approve -> Finalize chứng từ gốc
            await FinalizeDocumentAsync(approval.DocumentType, approval.DocumentId);
        }
    }

    public async Task RejectAsync(int approvalId, int approverId, string comment)
    {
        var approval = await _approvalRepo.GetByIdAsync(approvalId)
            ?? throw new KeyNotFoundException($"Không tìm thấy phiếu trình duyệt ID={approvalId}.");

        if (approval.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Phiếu này đã được xử lý trước đó.");

        approval.ApproverId = approverId;
        approval.Comment = comment;
        approval.ApprovedAt = DateTime.UtcNow;
        approval.Status = ApprovalStatus.Rejected;
        _approvalRepo.Update(approval);
        await _approvalRepo.SaveChangesAsync();
    }

    private async Task FinalizeDocumentAsync(DocumentType docType, int docId)
    {
        using var scope = _serviceProvider.CreateScope();

        switch (docType)
        {
            case DocumentType.Grn:
                var grnService = scope.ServiceProvider.GetRequiredService<IGrnService>();
                await grnService.FinalizeAsync(docId);
                break;

            case DocumentType.Gdn:
                var gdnService = scope.ServiceProvider.GetRequiredService<IGdnService>();
                await gdnService.FinalizeAsync(docId);
                break;

            case DocumentType.Transfer:
                var transferService = scope.ServiceProvider.GetRequiredService<ITransferOrderService>();
                await transferService.FinalizeAsync(docId);
                break;

            case DocumentType.StockAdjustment:
                var adjService = scope.ServiceProvider.GetRequiredService<IStockAdjustmentService>();
                await adjService.FinalizeAsync(docId);
                break;
        }
    }
}
