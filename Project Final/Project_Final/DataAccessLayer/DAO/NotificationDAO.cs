using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.System;

namespace DataAccessLayer.DAO;

public class NotificationDAO : GenericDAO<Notification>
{
    public NotificationDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false, int limit = 50)
    {
        var query = _dbSet.Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notif = await GetByIdAsync(notificationId);
        if (notif != null && !notif.IsRead)
        {
            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            _dbSet.Update(notif);
        }
    }
}
