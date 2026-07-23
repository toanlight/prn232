using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Approvals;

public class ApprovalWorkflow : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    public byte Level { get; set; }              // 1 = L1 Manager, 2 = L2 Director
    public string ApproverRoleCode { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Approval record polymorphic — liên kết tới GRN/GDN/Transfer/Adjustment
/// qua cặp (DocumentType, DocumentId) thay vì FK cứng, để dùng chung 1 bảng
/// cho toàn bộ approval workflow trong hệ thống.
/// </summary>
public class Approval : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    public int DocumentId { get; set; }
    public ApprovalLevel Level { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public int? ApproverId { get; set; }
    public User? Approver { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
