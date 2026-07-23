using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.System;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string NotifType { get; set; } = default!;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = default!;

    [StringLength(200)]
    public string? TitleEn { get; set; }

    [Required]
    [StringLength(1000)]
    public string Body { get; set; } = default!;

    [StringLength(1000)]
    public string? BodyEn { get; set; }

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmailLog : BaseEntity
{
    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string RecipientEmail { get; set; } = default!;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = default!;

    [StringLength(100)]
    public string? TemplateName { get; set; }

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "QUEUED";

    public DateTime? SentAt { get; set; }
    public byte RetryCount { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class SystemSetting : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string SettingKey { get; set; } = default!;

    [StringLength(2000)]
    public string? SettingValue { get; set; }

    [Required]
    [StringLength(20)]
    public string DataType { get; set; } = "string";

    [StringLength(255)]
    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
