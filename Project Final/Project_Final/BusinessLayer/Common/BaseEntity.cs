namespace BusinessLayer.Common;

/// <summary>
/// Lớp cơ sở cho tất cả Entity có khóa chính dạng số nguyên (int)
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
