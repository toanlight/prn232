using BusinessLayer.Entities.System;

namespace Repositories.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false, int limit = 50);
    Task MarkAsReadAsync(int notificationId);
}
