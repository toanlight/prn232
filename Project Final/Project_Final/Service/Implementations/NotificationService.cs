using BusinessLayer.Entities.System;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifRepo;

    public NotificationService(INotificationRepository notifRepo)
        => _notifRepo = notifRepo;

    public async Task<List<Notification>> GetByUserIdAsync(int userId, bool unreadFirst = true, int limit = 50)
        => await _notifRepo.GetByUserIdAsync(userId, unreadOnly: false, limit);

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        var unread = await _notifRepo.GetByUserIdAsync(userId, unreadOnly: true, 999);
        return unread.Count;
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notif = await _notifRepo.GetByIdAsync(notificationId)
            ?? throw new KeyNotFoundException($"Không tìm thấy thông báo ID={notificationId}.");

        if (notif.UserId != userId)
            throw new UnauthorizedAccessException("Không có quyền truy cập thông báo này.");

        await _notifRepo.MarkAsReadAsync(notificationId);
        await _notifRepo.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var unread = await _notifRepo.GetByUserIdAsync(userId, unreadOnly: true, 999);
        foreach (var notif in unread)
        {
            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            _notifRepo.Update(notif);
        }
        await _notifRepo.SaveChangesAsync();
    }

    public async Task SendAsync(int userId, string title, string message, string type = "INFO")
    {
        var notif = new Notification
        {
            UserId = userId,
            Title = title,
            Body = message,
            NotifType = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notifRepo.AddAsync(notif);
        await _notifRepo.SaveChangesAsync();
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Tích hợp SMTP client trong Phase 4
        // Hiện tại chỉ log để tránh break build
        await Task.CompletedTask;
    }
}
