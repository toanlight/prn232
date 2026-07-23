# WMS — ERD chi tiết (Code-First .NET / EF Core 8)
# Version 1.0

Tài liệu này cung cấp toàn bộ Entity Models (C# POCO classes) và Fluent API
configuration để dùng trực tiếp cho ASP.NET Web API với Entity Framework Core
Code-First approach. Namespace giả định: `WMS.Domain.Entities`.

---

## 1. Sơ đồ ERD tổng quan (Text Diagram)

```
Roles 1───N UserRoles N───1 Users
                                │1
                                │
                    ┌───────────┼──────────────┬─────────────┐
                    │N          │N              │N            │N
              Warehouses   Products      Suppliers       Customers
                    │1           │1            │1              │1
                    │N           │             │N              │N
                  Zones          │        Batches(FK)      (GDN, DispatchRequest FK)
                    │1           │N          │1
                    │N           │           │N
                  Racks    ProductCategories BinStocks N───1 Bins
                    │1                                          │1
                    │N                                          │N
                  Shelves ──────────────────────────────────  Shelves
                    │1
                    │N
                   Bins

GRN(1)───N GRNLines───N:1 Products / Batches / Bins
GDN(1)───N GDNLines───N:1 Products / Batches / Bins
TransferOrder(1)───N TransferOrderLines───N:1 Products/Batches/FromBin/ToBin
StockCount(1)───N StockCountLines───N:1 Bins/Products/Batches
StockAdjustment(1)───N StockAdjustmentLines───N:1 Bins/Products/Batches

Approvals: polymorphic liên kết qua (DocumentType, DocumentId) tới
           GRN / GDN / TransferOrder / StockAdjustment

StockTransactions: ledger ghi nhận MỌI biến động (immutable, append-only)
```

---

## 2. Base Classes

```csharp
namespace WMS.Domain.Common;

/// <summary>Base entity cho các bảng có Id kiểu int</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}

/// <summary>Interface đánh dấu entity có audit field tạo/sửa</summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

/// <summary>Enum trạng thái chung cho các chứng từ có phê duyệt 2 cấp</summary>
public enum DocumentStatus
{
    Draft = 0,
    PendingL1 = 1,
    PendingL2 = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    // GDN-specific extension states
    Picking = 6,
    Picked = 7,
    Delivered = 8,
    // Transfer-specific
    Completed = 9
}

public enum ApprovalStatus { Pending = 0, Approved = 1, Rejected = 2 }
public enum ApprovalLevel { L1Manager = 1, L2Director = 2 }
public enum CustomerType { B2BService = 0, Consignee = 1 }
public enum SupplierStatus { Active = 0, Inactive = 1, Blacklisted = 2 }
public enum BatchStatus { Active = 0, Expired = 1, Consumed = 2, Recalled = 3 }
public enum CountType { Full = 0, Periodic = 1, Cycle = 2 }
public enum StockTxnType
{
    GrnIn, GrnReturnOut, GdnOut, GdnReturnIn,
    TransferOut, TransferIn, AdjIn, AdjOut
}
public enum DocumentType { Grn, Gdn, Transfer, StockAdjustment }
```

---

## 3. Identity & Access Models

```csharp
namespace WMS.Domain.Entities;

public class Role : BaseEntity
{
    public string RoleCode { get; set; } = default!;   // SYS_ADMIN, DIRECTOR, WH_MANAGER...
    public string RoleName { get; set; } = default!;    // Tiếng Việt
    public string RoleNameEn { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class User : BaseEntity, IAuditable
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? FullNameEn { get; set; }
    public string? Phone { get; set; }
    public string PreferredLang { get; set; } = "vi";   // vi | en
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = default!;
    public DateTime AssignedAt { get; set; }
    public int? AssignedBy { get; set; }
}

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 4. Warehouse Structure Models (5 cấp)

```csharp
public class Warehouse : BaseEntity, IAuditable
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? Address { get; set; }
    public int? ManagerUserId { get; set; }
    public User? ManagerUser { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}

public class Zone : BaseEntity
{
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? ZoneType { get; set; }   // COLD, DRY, HAZMAT, GENERAL
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Rack> Racks { get; set; } = new List<Rack>();
}

public class Rack : BaseEntity
{
    public int ZoneId { get; set; }
    public Zone Zone { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
}

public class Shelf : BaseEntity
{
    public int RackId { get; set; }
    public Rack Rack { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Bin> Bins { get; set; } = new List<Bin>();
}

public class Bin : BaseEntity
{
    public int ShelfId { get; set; }
    public Shelf Shelf { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal? MaxCapacity { get; set; }
    public string? CapacityUnit { get; set; }   // KG, CBM, UNIT
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
}
```

---

## 5. Product Catalog Models

```csharp
public class ProductCategory : BaseEntity
{
    public int? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class UnitOfMeasure : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Product : BaseEntity, IAuditable
{
    public string SKU { get; set; } = default!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; } = default!;
    public int UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Stock control
    public decimal MinStock { get; set; }
    public decimal ReorderPoint { get; set; }

    // Batch config
    public bool IsBatchTracked { get; set; } = true;
    public bool IsExpiryTracked { get; set; }
    public int ExpiryWarningDays { get; set; } = 30;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
}
```

---

## 6. Supplier & Customer Models

```csharp
public class Supplier : BaseEntity, IAuditable
{
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

    public SupplierStatus Status { get; set; } = SupplierStatus.Active;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

public class Customer : BaseEntity, IAuditable
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public CustomerType CustomerType { get; set; } = CustomerType.Consignee;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }

    // For B2BService type
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? ServiceTerms { get; set; }
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GoodsDispatchNote> DispatchNotes { get; set; } = new List<GoodsDispatchNote>();
}
```

---

## 7. Batch & Stock Models (CORE)

```csharp
public class Batch : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string LotNumber { get; set; } = default!;
    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal InitialQty { get; set; }
    public string? Notes { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Active;
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;

    public ICollection<BinStock> BinStocks { get; set; } = new List<BinStock>();
    public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
}

/// <summary>
/// BẢNG TỒN KHO CHÍNH — mỗi record = số lượng của 1 Batch tại 1 Bin cụ thể.
/// Đây là "single source of truth" cho tồn kho realtime.
/// Unique constraint: (BinId, BatchId)
/// </summary>
public class BinStock : BaseEntity
{
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public int ProductId { get; set; }          // Denormalized cho query performance
    public Product Product { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal ReservedQty { get; set; }     // Giữ chỗ bởi GDN đang PENDING/APPROVED
    public DateTime UpdatedAt { get; set; }

    [NotMapped]
    public decimal AvailableQty => Quantity - ReservedQty;
}

/// <summary>
/// Sổ cái bất biến (append-only ledger) ghi lại MỌI biến động tồn kho.
/// Dùng cho audit trail và báo cáo Nhập-Xuất-Tồn.
/// </summary>
public class StockTransaction
{
    public long Id { get; set; }   // bigint vì đây là bảng lớn nhất, tăng nhanh nhất
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public StockTxnType TxnType { get; set; }
    public DocumentType DocumentType { get; set; }
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = default!;
    public decimal Quantity { get; set; }        // luôn dương; chiều +/- xác định bởi TxnType
    public decimal QtyBefore { get; set; }
    public decimal QtyAfter { get; set; }
    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
```

---

## 8. Approval Engine Model (Polymorphic)

```csharp
public class ApprovalWorkflow : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    public byte Level { get; set; }              // 1 = L1 Manager, 2 = L2 Director
    public string ApproverRoleCode { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Approval record polymorphic — liên kết tới GRN/GDN/Transfer/Adjustment
/// qua cặp (DocumentType, DocumentId) thay vì FK cứng, để dùng chung 1 bảng
/// cho toàn bộ approval workflow trong hệ thống.
/// </summary>
public class Approval : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    public int DocumentId { get; set; }
    public ApprovalLevel Level { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public int? ApproverId { get; set; }
    public User? Approver { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 9. Purchase Order & GRN Models

```csharp
public class PurchaseOrder : BaseEntity, IAuditable
{
    public string PONumber { get; set; } = default!;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly OrderDate { get; set; }
    public DateOnly? ExpectedDate { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "DRAFT";  // DRAFT|SUBMITTED|PARTIALLY_RECEIVED|RECEIVED|CANCELLED
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<POLine> Lines { get; set; } = new List<POLine>();
    public ICollection<GoodsReceiptNote> GRNs { get; set; } = new List<GoodsReceiptNote>();
}

public class POLine : BaseEntity
{
    public int POId { get; set; }
    public PurchaseOrder PO { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public class GoodsReceiptNote : BaseEntity
{
    public string GRNNumber { get; set; } = default!;
    public int? POId { get; set; }
    public PurchaseOrder? PO { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly ReceiptDate { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrls { get; set; }   // JSON array

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public bool IsReturn { get; set; }
    public int? ParentGRNId { get; set; }
    public GoodsReceiptNote? ParentGRN { get; set; }

    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<GRNLine> Lines { get; set; } = new List<GRNLine>();
}

public class GRNLine : BaseEntity
{
    public int GRNId { get; set; }
    public GoodsReceiptNote GRN { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int? BatchId { get; set; }             // Null cho đến khi Batch được tạo/khớp
    public Batch? Batch { get; set; }
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public decimal Quantity { get; set; }

    // Thông tin lô — dùng để tạo Batch mới nếu chưa tồn tại
    public string? LotNumber { get; set; }
    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
}
```

---

## 10. Dispatch Request & GDN Models

```csharp
public class DispatchRequest : BaseEntity
{
    public string RequestNumber { get; set; } = default!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly RequestDate { get; set; }
    public DateOnly? RequiredDate { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "OPEN";   // OPEN|IN_PROGRESS|FULFILLED|CANCELLED
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public ICollection<GoodsDispatchNote> GDNs { get; set; } = new List<GoodsDispatchNote>();
}

public class GoodsDispatchNote : BaseEntity
{
    public string GDNNumber { get; set; } = default!;
    public int? RequestId { get; set; }
    public DispatchRequest? Request { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly DispatchDate { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrls { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public bool IsReturn { get; set; }
    public int? ParentGDNId { get; set; }
    public GoodsDispatchNote? ParentGDN { get; set; }

    public DateTime? PickedAt { get; set; }
    public int? PickedBy { get; set; }
    public User? PickedByUser { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int? DeliveredBy { get; set; }
    public User? DeliveredByUser { get; set; }

    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GDNLine> Lines { get; set; } = new List<GDNLine>();
}

public class GDNLine : BaseEntity
{
    public int GDNId { get; set; }
    public GoodsDispatchNote GDN { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }               // Batch được chọn theo FEFO
    public Batch Batch { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public decimal RequestedQty { get; set; }
    public decimal? PickedQty { get; set; }         // Số lượng thực tế lấy
    public string? Notes { get; set; }
}
```

---

## 11. Transfer Order Models

```csharp
public class TransferOrder : BaseEntity
{
    public string TransferNumber { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly TransferDate { get; set; }
    public string? Reason { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<TransferOrderLine> Lines { get; set; } = new List<TransferOrderLine>();
}

public class TransferOrderLine : BaseEntity
{
    public int TransferId { get; set; }
    public TransferOrder Transfer { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public int FromBinId { get; set; }
    public Bin FromBin { get; set; } = default!;
    public int ToBinId { get; set; }
    public Bin ToBin { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}
```

---

## 12. Stock Count & Adjustment Models

```csharp
public class StockCount : BaseEntity
{
    public string CountNumber { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public CountType CountType { get; set; }
    public int? ZoneId { get; set; }               // Null = toàn kho
    public Zone? Zone { get; set; }
    public int? RackId { get; set; }
    public Rack? Rack { get; set; }
    public DateOnly CountDate { get; set; }
    public int PlannedBy { get; set; }
    public User PlannedByUser { get; set; } = default!;
    public string? Notes { get; set; }
    // PLANNED|IN_PROGRESS|COMPLETED|ADJUSTMENT_PENDING|CLOSED
    public string Status { get; set; } = "PLANNED";
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<StockCountLine> Lines { get; set; } = new List<StockCountLine>();
    public ICollection<StockAdjustment> Adjustments { get; set; } = new List<StockAdjustment>();
}

public class StockCountLine : BaseEntity
{
    public int CountId { get; set; }
    public StockCount Count { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public decimal SystemQty { get; set; }          // Snapshot khi bắt đầu kiểm
    public decimal? ActualQty { get; set; }          // WH_STAFF nhập
    [NotMapped]
    public decimal Variance => (ActualQty ?? 0) - SystemQty;
    public int? CountedBy { get; set; }
    public User? CountedByUser { get; set; }
    public DateTime? CountedAt { get; set; }
    public string? Notes { get; set; }
}

public class StockAdjustment : BaseEntity
{
    public string AdjNumber { get; set; } = default!;
    public int? CountId { get; set; }
    public StockCount? Count { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<StockAdjustmentLine> Lines { get; set; } = new List<StockAdjustmentLine>();
}

public class StockAdjustmentLine : BaseEntity
{
    public int AdjustmentId { get; set; }
    public StockAdjustment Adjustment { get; set; } = default!;
    public int BinId { get; set; }
    public Bin Bin { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;
    public decimal BeforeQty { get; set; }
    public decimal AfterQty { get; set; }
    [NotMapped]
    public decimal DeltaQty => AfterQty - BeforeQty;
    public string? Notes { get; set; }
}
```

---

## 13. Notification & System Models

```csharp
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
```

---

## 14. DbContext & Fluent API Configuration

```csharp
namespace WMS.Infrastructure.Data;

public class WmsDbContext : DbContext
{
    public WmsDbContext(DbContextOptions<WmsDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Rack> Racks => Set<Rack>();
    public DbSet<Shelf> Shelves => Set<Shelf>();
    public DbSet<Bin> Bins => Set<Bin>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<BinStock> BinStocks => Set<BinStock>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<Approval> Approvals => Set<Approval>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<POLine> POLines => Set<POLine>();
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<GRNLine> GRNLines => Set<GRNLine>();

    public DbSet<DispatchRequest> DispatchRequests => Set<DispatchRequest>();
    public DbSet<GoodsDispatchNote> GoodsDispatchNotes => Set<GoodsDispatchNote>();
    public DbSet<GDNLine> GDNLines => Set<GDNLine>();

    public DbSet<TransferOrder> TransferOrders => Set<TransferOrder>();
    public DbSet<TransferOrderLine> TransferOrderLines => Set<TransferOrderLine>();

    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockCountLine> StockCountLines => Set<StockCountLine>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WmsDbContext).Assembly);

        // ---- Unique Constraints ----
        modelBuilder.Entity<Role>().HasIndex(x => x.RoleCode).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<UserRole>().HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();

        modelBuilder.Entity<Warehouse>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Zone>().HasIndex(x => new { x.WarehouseId, x.Code }).IsUnique();
        modelBuilder.Entity<Rack>().HasIndex(x => new { x.ZoneId, x.Code }).IsUnique();
        modelBuilder.Entity<Shelf>().HasIndex(x => new { x.RackId, x.Code }).IsUnique();
        modelBuilder.Entity<Bin>().HasIndex(x => new { x.ShelfId, x.Code }).IsUnique();

        modelBuilder.Entity<Product>().HasIndex(x => x.SKU).IsUnique();
        modelBuilder.Entity<ProductCategory>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<UnitOfMeasure>().HasIndex(x => x.Code).IsUnique();

        modelBuilder.Entity<Supplier>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(x => x.Code).IsUnique();

        modelBuilder.Entity<BinStock>().HasIndex(x => new { x.BinId, x.BatchId }).IsUnique();

        modelBuilder.Entity<PurchaseOrder>().HasIndex(x => x.PONumber).IsUnique();
        modelBuilder.Entity<GoodsReceiptNote>().HasIndex(x => x.GRNNumber).IsUnique();
        modelBuilder.Entity<DispatchRequest>().HasIndex(x => x.RequestNumber).IsUnique();
        modelBuilder.Entity<GoodsDispatchNote>().HasIndex(x => x.GDNNumber).IsUnique();
        modelBuilder.Entity<TransferOrder>().HasIndex(x => x.TransferNumber).IsUnique();
        modelBuilder.Entity<StockCount>().HasIndex(x => x.CountNumber).IsUnique();
        modelBuilder.Entity<StockAdjustment>().HasIndex(x => x.AdjNumber).IsUnique();

        modelBuilder.Entity<ApprovalWorkflow>()
            .HasIndex(x => new { x.DocumentType, x.Level }).IsUnique();

        modelBuilder.Entity<SystemSetting>().HasIndex(x => x.SettingKey).IsUnique();

        // ---- Precision cho các cột decimal ----
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,4)");
        }

        // ---- Restrict cascade delete mặc định (tránh multiple cascade paths) ----
        foreach (var fk in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
```

### Fluent API Configuration riêng cho quan hệ đặc biệt (self-referencing, multiple FK cùng bảng)

```csharp
namespace WMS.Infrastructure.Data.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        // Self-referencing tree
        builder.HasOne(x => x.Parent)
               .WithMany(x => x.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TransferOrderLineConfiguration : IEntityTypeConfiguration<TransferOrderLine>
{
    public void Configure(EntityTypeBuilder<TransferOrderLine> builder)
    {
        // 2 FK khác nhau cùng trỏ về bảng Bins => phải chỉ định rõ để tránh
        // EF Core tự sinh shadow FK sai
        builder.HasOne(x => x.FromBin)
               .WithMany()
               .HasForeignKey(x => x.FromBinId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToBin)
               .WithMany()
               .HasForeignKey(x => x.ToBinId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GoodsReceiptNoteConfiguration : IEntityTypeConfiguration<GoodsReceiptNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNote> builder)
    {
        // Self-referencing cho Return GRN
        builder.HasOne(x => x.ParentGRN)
               .WithMany()
               .HasForeignKey(x => x.ParentGRNId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20);
    }
}

public class GoodsDispatchNoteConfiguration : IEntityTypeConfiguration<GoodsDispatchNote>
{
    public void Configure(EntityTypeBuilder<GoodsDispatchNote> builder)
    {
        builder.HasOne(x => x.ParentGDN)
               .WithMany()
               .HasForeignKey(x => x.ParentGDNId)
               .OnDelete(DeleteBehavior.Restrict);

        // 2 User FK khác nhau (PickedBy / DeliveredBy / CreatedBy) trên cùng bảng Users
        builder.HasOne(x => x.PickedByUser)
               .WithMany()
               .HasForeignKey(x => x.PickedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DeliveredByUser)
               .WithMany()
               .HasForeignKey(x => x.DeliveredBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
               .WithMany()
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20);
    }
}

public class BinStockConfiguration : IEntityTypeConfiguration<BinStock>
{
    public void Configure(EntityTypeBuilder<BinStock> builder)
    {
        builder.HasIndex(x => new { x.BinId, x.BatchId }).IsUnique();
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(x => x.ReservedQty).HasColumnType("decimal(18,4)");
        // AvailableQty is [NotMapped] computed property, không cần config
    }
}

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        // bigint identity vì bảng ledger tăng nhanh nhất
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasIndex(x => new { x.ProductId, x.CreatedAt });
        builder.HasIndex(x => x.BatchId);
        builder.HasIndex(x => new { x.DocumentType, x.DocumentId });

        builder.Property(x => x.TxnType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(20);
    }
}

public class ApprovalConfiguration : IEntityTypeConfiguration<Approval>
{
    public void Configure(EntityTypeBuilder<Approval> builder)
    {
        // Polymorphic — không có FK cứng, chỉ index để query nhanh
        builder.HasIndex(x => new { x.DocumentType, x.DocumentId });
        builder.HasIndex(x => x.Status)
               .HasFilter("[Status] = 'Pending'");

        builder.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    }
}
```

---

## 15. Quan hệ chính (Relationship Summary Table)

| Parent | Child | Type | Ghi chú |
|---|---|---|---|
| Warehouse | Zone | 1-N | Cascade Restrict |
| Zone | Rack | 1-N | |
| Rack | Shelf | 1-N | |
| Shelf | Bin | 1-N | |
| ProductCategory | ProductCategory | 1-N (self) | Tree danh mục |
| ProductCategory | Product | 1-N | |
| Product | Batch | 1-N | |
| Product | BinStock | 1-N | Denormalized FK để query nhanh |
| Bin | BinStock | 1-N | |
| Batch | BinStock | 1-N | **Unique(BinId, BatchId)** |
| Supplier | Batch | 1-N (optional) | |
| Supplier | PurchaseOrder | 1-N | |
| PurchaseOrder | POLine | 1-N | |
| PurchaseOrder | GoodsReceiptNote | 1-N (optional) | GRN có thể không từ PO |
| GoodsReceiptNote | GRNLine | 1-N | |
| GoodsReceiptNote | GoodsReceiptNote | 1-N (self, optional) | Return GRN → Parent GRN |
| Customer | DispatchRequest | 1-N | |
| Customer | GoodsDispatchNote | 1-N | |
| DispatchRequest | GoodsDispatchNote | 1-N (optional) | |
| GoodsDispatchNote | GDNLine | 1-N | |
| GoodsDispatchNote | GoodsDispatchNote | 1-N (self, optional) | Return GDN → Parent GDN |
| TransferOrder | TransferOrderLine | 1-N | Line có 2 FK tới Bin (From/To) |
| StockCount | StockCountLine | 1-N | |
| StockCount | StockAdjustment | 1-N (optional) | |
| StockAdjustment | StockAdjustmentLine | 1-N | |
| User | UserRole | 1-N | |
| Role | UserRole | 1-N | |
| — | Approval | Polymorphic | Liên kết qua (DocumentType, DocumentId), KHÔNG có FK cứng |

---

## 16. Lưu ý khi Implement

1. **BinStock là bảng trung tâm.** Mọi thao tác nhập/xuất/chuyển/điều chỉnh đều phải update qua Service layer (không update trực tiếp), đảm bảo đồng thời ghi `StockTransaction` để giữ audit trail đầy đủ.

2. **FEFO Query** nên implement bằng LINQ hoặc raw SQL view (`vw_FEFOStock`) — xem file `03_Database_Schema.sql` — vì cần `ROW_NUMBER() OVER (PARTITION BY ProductId ORDER BY ExpiryDate)`.

3. **Approval polymorphic** không dùng FK cứng để tránh phải tạo 4 bảng Approval riêng biệt (GRNApproval, GDNApproval...). Trade-off: mất referential integrity ở DB level, cần enforce ở Service layer.

4. **DateOnly vs DateTime**: dùng `DateOnly` (.NET 6+) cho các trường chỉ cần ngày (MfgDate, ExpiryDate, OrderDate...) để tránh vấn đề timezone; dùng `DateTime` cho các trường cần cả giờ (CreatedAt, ApprovedAt...).

5. **Enum lưu dạng string** (`HasConversion<string>()`) thay vì int để dễ đọc trực tiếp trong DB khi debug, đổi lại tốn thêm vài byte lưu trữ — chấp nhận được với quy mô <5.000 SKU / <200 giao dịch/ngày.

6. **Concurrency control**: khuyến nghị thêm `RowVersion` (`byte[]`, `[Timestamp]`) vào `BinStock` để tránh race condition khi 2 giao dịch cùng cập nhật 1 Bin đồng thời (ví dụ 2 nhân viên cùng picking từ 1 Bin).
