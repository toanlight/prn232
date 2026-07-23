using BusinessLayer.Entities.System;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly NotificationDAO _notifDao;

    public NotificationRepository(WmsDbContext context) : base(context)
    {
        _notifDao = new NotificationDAO(context);
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false, int limit = 50) => await _notifDao.GetByUserIdAsync(userId, unreadOnly, limit);
    public async Task MarkAsReadAsync(int notificationId) => await _notifDao.MarkAsReadAsync(notificationId);
}
