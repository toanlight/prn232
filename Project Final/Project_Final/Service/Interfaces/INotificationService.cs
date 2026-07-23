using BusinessLayer.Entities.System;

namespace Service.Interfaces;

public interface INotificationService
{
    Task<List<Notification>> GetByUserIdAsync(int userId, bool unreadFirst = true, int limit = 50);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task SendAsync(int userId, string title, string message, string type = "INFO");
    Task SendEmailAsync(string to, string subject, string body);
}
