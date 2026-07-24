using System.Text.Json.Serialization;

namespace WMS.Web.Models;

public class ApiResponseModel<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
}

public class PagedResponseModel<T>
{
    public bool Success { get; set; }

    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();

    [JsonIgnore]
    public List<T> Data => Items;

    public string? Message { get; set; }
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}

public class SupplierItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CustomerItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string CustomerType { get; set; } = default!;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? ServiceTerms { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class ProductItemViewModel
{
    public int Id { get; set; }
    public string SKU { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? Barcode { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int UomId { get; set; }
    public string? UomCode { get; set; }
    public string? UomName { get; set; }
    public string? Description { get; set; }
    public decimal MinStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public bool IsBatchTracked { get; set; }
    public bool IsExpiryTracked { get; set; }
    public int ExpiryWarningDays { get; set; } = 30;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CategoryItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class UomItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
}

public class WarehouseItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? Address { get; set; }
    public int? ManagerUserId { get; set; }
    public string? ManagerUserName { get; set; }
    public int ZoneCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BatchItemViewModel
{
    public int Id { get; set; }
    public string LotNumber { get; set; } = default!;
    public string? ProductName { get; set; }
    public string? SupplierName { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal InitialQty { get; set; }
    public string Status { get; set; } = default!;
}

public class DashboardMetricsViewModel
{
    public int TotalProducts { get; set; }
    public int LowStockAlerts { get; set; }
    public int ExpiringBatches { get; set; }
    public int PendingApprovals { get; set; }
}

public class ApprovalItemViewModel
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = default!;
    public int DocumentId { get; set; }
    public int Level { get; set; }
    public string Status { get; set; } = default!;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockItemViewModel
{
    public int BinId { get; set; }
    public string BinCode { get; set; } = default!;
    public string SKU { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string LotNumber { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal ReservedQty { get; set; }
    public decimal AvailableQty { get; set; }
}

public class UserItemViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}

public class GrnItemViewModel
{
    public int Id { get; set; }
    public string GRNNumber { get; set; } = default!;
    public string SupplierName { get; set; } = default!;
    public string WarehouseName { get; set; } = default!;
    public DateOnly ReceiptDate { get; set; }
    public string Status { get; set; } = default!;
}

public class GdnItemViewModel
{
    public int Id { get; set; }
    public string GDNNumber { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string WarehouseName { get; set; } = default!;
    public DateOnly DispatchDate { get; set; }
    public string Status { get; set; } = default!;
}
