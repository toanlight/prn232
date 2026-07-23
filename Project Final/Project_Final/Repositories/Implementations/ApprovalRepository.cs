using BusinessLayer.Entities.Approvals;
using BusinessLayer.Enums;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class ApprovalRepository : GenericRepository<Approval>, IApprovalRepository
{
    private readonly ApprovalDAO _approvalDao;

    public ApprovalRepository(WmsDbContext context) : base(context)
    {
        _approvalDao = new ApprovalDAO(context);
    }

    public async Task<List<Approval>> GetByDocumentAsync(DocumentType documentType, int documentId) => await _approvalDao.GetByDocumentAsync(documentType, documentId);
    public async Task<List<Approval>> GetPendingApprovalsByApproverIdAsync(int approverId) => await _approvalDao.GetPendingApprovalsByApproverIdAsync(approverId);
}
