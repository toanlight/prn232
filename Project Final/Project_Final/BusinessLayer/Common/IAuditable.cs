namespace BusinessLayer.Common;

/// <summary>
/// Interface đánh dấu Entity lưu vết thời gian tạo/chỉnh sửa
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
