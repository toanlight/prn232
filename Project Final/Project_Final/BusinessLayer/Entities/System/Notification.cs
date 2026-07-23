using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.System;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    // LOW_STOCK|EXPIRY_WARNING|EXPIRED|APPROVAL_PENDING|APPROVAL_RESULT|COUNT_VARIANCE|DELIVERY_CONFIRM
    public string NotifType { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? TitleEn { get; set; }
    public string Body { get; set; } = default!;
    public string? BodyEn { get; set; }
    public string? ReferenceType { get; set; }   // GRN|GDN|TRANSFER|PRODUCT|BATCH
    public int? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmailLog : BaseEntity
{
    public string RecipientEmail { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string? TemplateName { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string Status { get; set; } = "QUEUED";  // QUEUED|SENT|FAILED
    public DateTime? SentAt { get; set; }
    public byte RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SystemSetting : BaseEntity
{
    public string SettingKey { get; set; } = default!;
    public string? SettingValue { get; set; }
    public string DataType { get; set; } = "string";  // string|int|bool|json
    public string? Description { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
