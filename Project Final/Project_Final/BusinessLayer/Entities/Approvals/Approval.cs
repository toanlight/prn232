using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Approvals;

public class ApprovalWorkflow : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    public byte Level { get; set; }

    [Required]
    [StringLength(50)]
    public string ApproverRoleCode { get; set; } = default!;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class Approval : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    public int DocumentId { get; set; }
    public ApprovalLevel Level { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public int? ApproverId { get; set; }
    [ForeignKey(nameof(ApproverId))]
    public User? Approver { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}
