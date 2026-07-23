using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Approvals;
using BusinessLayer.Enums;

namespace DataAccessLayer.DAO;

public class ApprovalDAO : GenericDAO<Approval>
{
    public ApprovalDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<List<Approval>> GetByDocumentAsync(DocumentType documentType, int documentId)
    {
        return await _dbSet
            .Include(a => a.Approver)
            .Where(a => a.DocumentType == documentType && a.DocumentId == documentId)
            .OrderBy(a => a.Level)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Approval>> GetPendingApprovalsByApproverIdAsync(int approverId)
    {
        return await _dbSet
            .Include(a => a.Approver)
            .Where(a => a.ApproverId == approverId && a.Status == ApprovalStatus.Pending)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
