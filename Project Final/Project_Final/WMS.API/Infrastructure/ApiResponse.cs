namespace WMS.API.Infrastructure;

/// <summary>
/// Generic API Response wrapper for consistent JSON output.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message ?? "Created successfully",
        StatusCode = 201
    };

    public static ApiResponse<T> Error(string message, int statusCode = 400) => new()
    {
        Success = false,
        Data = default,
        Message = message,
        StatusCode = statusCode
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkMessage(string message = "Success") => new()
    {
        Success = true,
        Message = message,
        StatusCode = 200
    };

    public static new ApiResponse Error(string message, int statusCode = 400) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode
    };
}

public class PagedResponse<T>
{
    public bool Success { get; set; } = true;
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public static PagedResponse<T> From(List<T> items, int totalCount, int pageIndex, int pageSize) => new()
    {
        Items = items,
        TotalCount = totalCount,
        PageIndex = pageIndex,
        PageSize = pageSize
    };
}
