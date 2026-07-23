using BusinessLayer.Entities.Approvals;
using BusinessLayer.Enums;

namespace Repositories.Interfaces;

public interface IApprovalRepository : IGenericRepository<Approval>
{
    Task<List<Approval>> GetByDocumentAsync(DocumentType documentType, int documentId);
    Task<List<Approval>> GetPendingApprovalsByApproverIdAsync(int approverId);
}
