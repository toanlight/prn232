# WMS — Tài liệu Chi tiết Tác dụng & Vai trò của các Entity Models

Tài liệu này giải thích chi tiết mục đích, vai trò nghiệp vụ, ràng buộc dữ liệu và mối quan hệ của toàn bộ **Entity Models** nằm trong project `BusinessLayer` của hệ thống Quản lý Kho Hàng (WMS).

---

## Danh mục Phân nhóm Entity Models

```
BusinessLayer/Entities/
├── Identity/              <- Quản lý Người dùng & Phân quyền (4 models)
├── Warehouse/             <- Cấu trúc Kho 5 cấp (5 models)
├── Products/              <- Danh mục Sản phẩm & Đơn vị tính (3 models)
├── Partners/              <- Nhà cung cấp & Khách hàng (2 models)
├── Stock/                 <- Lô hàng & Tồn kho Realtime (3 models)
├── Orders/                <- Đơn hàng & Chứng từ Nhập/Xuất/Chuyển/Kiểm kê (13 models)
├── Approvals/             <- Động cơ Phê duyệt 2 cấp (2 models)
└── System/                <- Thông báo, Mail log & Cấu hình (3 models)
```

---

## 1. Nhóm Quản lý Người dùng & Phân quyền (Identity & Access Control)

### 1.1 `User` (`BusinessLayer.Entities.Identity.User`)
- **Tác dụng & Vai trò**: Quản lý thông tin tài khoản người dùng đăng nhập hệ thống (Trưởng kho, Nhân viên kho, Giám đốc, Nhân viên mua hàng, Sales...).
- **Các trường chính**:
  - `Username`, `Email`: Mã định danh đăng nhập (độc nhất `IsUnique`).
  - `PasswordHash`: Mật khẩu được mã hóa an toàn.
  - `PreferredLang`: Ngôn ngữ giao diện ưu tiên (`vi` hoặc `en`).
  - `IsActive`: Trạng thái cho phép đăng nhập hay bị khóa tài khoản.
- **Quan hệ**: Liên kết N-N với `Role` thông qua `UserRole`; sở hữu danh sách `RefreshToken`.

### 1.2 `Role` (`BusinessLayer.Entities.Identity.Role`)
- **Tác dụng & Vai trò**: Định nghĩa các vai trò/quyền hạn trong hệ thống theo cơ chế RBAC (Role-Based Access Control).
- **Mã vai trò chuẩn (RoleCode)**: `SYS_ADMIN`, `DIRECTOR`, `WH_MANAGER`, `WH_STAFF`, `PURCHASING`, `SALES`, `INV_CTRL`.
- **Quan hệ**: Liên kết N-N với `User`.

### 1.3 `UserRole` (`BusinessLayer.Entities.Identity.UserRole`)
- **Tác dụng & Vai trò**: Bảng trung gian liên kết giữa `User` và `Role`, cho phép một người dùng có thể đảm nhận một hoặc nhiều vai trò cùng lúc.
- **Ràng buộc**: Khóa chính phức hợp `(UserId, RoleId)`.

### 1.4 `RefreshToken` (`BusinessLayer.Entities.Identity.RefreshToken`)
- **Tác dụng & Vai trò**: Quản lý token làm mới phiên đăng nhập JWT, hỗ trợ cơ chế Single Sign-On / Stateless Authentication mà không cần yêu cầu người dùng đăng nhập lại liên tục.
- **Các trường chính**: `Token`, `ExpiresAt`, `IsRevoked` (cờ thu hồi khi logout).

---

## 2. Nhóm Cấu trúc Kho hàng 5 cấp (Warehouse Infrastructure)

Hệ thống quản lý không gian lưu trữ theo cấu trúc phân cấp 5 tầng: **Warehouse $\rightarrow$ Zone $\rightarrow$ Rack $\rightarrow$ Shelf $\rightarrow$ Bin**.

### 2.1 `Warehouse` (`BusinessLayer.Entities.Warehouses.Warehouse`)
- **Tác dụng & Vai trò**: Đại diện cho một Kho hàng vật lý (Cấp 1).
- **Trường chính**: `Code` (mã kho), `Name`, `Address`, `ManagerUserId` (FK tới `User` làm Trưởng kho).
- **Quan hệ**: Chứa danh sách các `Zone`.

### 2.2 `Zone` (`BusinessLayer.Entities.Warehouses.Zone`)
- **Tác dụng & Vai trò**: Đại diện cho Phân khu chức năng trong kho (Cấp 2).
- **Phân loại (`ZoneType`)**: `COLD` (Kho lạnh), `DRY` (Kho khô), `HAZMAT` (Hóa chất/Hàng nguy hiểm), `GENERAL` (Kho chung).
- **Quan hệ**: Thuộc 1 `Warehouse`, chứa danh sách các `Rack`.

### 2.3 `Rack` (`BusinessLayer.Entities.Warehouses.Rack`)
- **Tác dụng & Vai trò**: Đại diện cho Dãy kệ / Giá đỡ (Cấp 3).
- **Quan hệ**: Thuộc 1 `Zone`, chứa danh sách các `Shelf`.

### 2.4 `Shelf` (`BusinessLayer.Entities.Warehouses.Shelf`)
- **Tác dụng & Vai trò**: Đại diện cho Tầng kệ / Ngăn kệ (Cấp 4).
- **Quan hệ**: Thuộc 1 `Rack`, chứa danh sách các `Bin`.

### 2.5 `Bin` (`BusinessLayer.Entities.Warehouses.Bin`)
- **Tác dụng & Vai trò**: Đại diện cho Ô chứa hàng / Vị trí lưu trữ nhỏ nhất (Cấp 5). Đây là vị trí chính xác để cất và lấy hàng.
- **Các trường chính**: `Code`, `MaxCapacity` (sức chứa tối đa), `CapacityUnit` (`KG`, `CBM`, `UNIT`).
- **Quan hệ**: Thuộc 1 `Shelf`, chứa danh sách các bản ghi tồn kho `BinStock`.

---

## 3. Nhóm Danh mục Sản phẩm & Đơn vị tính (Product Catalog)

### 3.1 `ProductCategory` (`BusinessLayer.Entities.Products.ProductCategory`)
- **Tác dụng & Vai trò**: Phân loại danh mục sản phẩm theo cấu trúc phân cấp hình cây (Self-referencing tree).
- **Trường chính**: `ParentId` (FK trỏ ngược về chính bản thân), `Code`, `Name`.

### 3.2 `UnitOfMeasure` (`BusinessLayer.Entities.Products.UnitOfMeasure`)
- **Tác dụng & Vai trò**: Định nghĩa các Đơn vị tính cho hàng hóa (Thùng, Hộp, Chai, Cái, KG, Cuộn...).

### 3.3 `Product` (`BusinessLayer.Entities.Products.Product`)
- **Tác dụng & Vai trò**: Quản lý thông tin masterdata của Sản phẩm / Hàng hóa.
- **Các trường chính**:
  - `SKU`: Mã sản phẩm (độc nhất).
  - `Barcode`: Mã vạch quét mã.
  - `MinStock`: Tồn kho tối thiểu (dùng để phát cảnh báo thiếu hàng `LowStock`).
  - `ReorderPoint`: Điểm đặt hàng lại.
  - `IsBatchTracked`: Bật/Tắt quản lý theo Lô sản xuất.
  - `IsExpiryTracked`: Bật/Tắt quản lý theo Hạn sử dụng.
  - `ExpiryWarningDays`: Số ngày trước khi hết hạn để phát cảnh báo (`ExpiryAlert`).

---

## 4. Nhóm Đối tác Kinh doanh (Business Partners)

### 4.1 `Supplier` (`BusinessLayer.Entities.Partners.Supplier`)
- **Tác dụng & Vai trò**: Quản lý thông tin Nhà cung cấp hàng hóa cho kho.
- **Trường chính**: `Code`, `Name`, `TaxCode`, `Address`, `Email`, `Phone`, `ContractNumber` (số hợp đồng), `Status` (`Active`, `Inactive`, `Blacklisted`).

### 4.2 `Customer` (`BusinessLayer.Entities.Partners.Customer`)
- **Tác dụng & Vai trò**: Quản lý thông tin Khách hàng nhập/xuất kho.
- **Phân loại (`CustomerType`)**:
  - `B2BService`: Khách hàng doanh nghiệp thuê dịch vụ lưu kho.
  - `Consignee`: Đơn vị/đại lý nhận hàng xuất kho.

---

## 5. Nhóm Lô & Tồn kho Realtime (Stock & Inventory Ledger)

### 5.1 `Batch` (`BusinessLayer.Entities.Stock.Batch`)
- **Tác dụng & Vai trò**: Quản lý thông tin Lô hàng nhập kho.
- **Các trường chính**:
  - `LotNumber`: Số lô sản xuất từ NCC.
  - `MfgDate`: Ngày sản xuất.
  - `ExpiryDate`: Ngày hết hạn (căn cứ quan trọng nhất để xuất hàng theo thuật toán FEFO - First Expired, First Out).
  - `Status`: `Active`, `Expired`, `Consumed`, `Recalled`.

### 5.2 `BinStock` (`BusinessLayer.Entities.Stock.BinStock`)
- **Tác dụng & Vai trò**: BẢNG TỒN KHO THỜI GIAN THỰC (Single Source of Truth). Lưu chính xác tại **Bin X** đang có bao nhiêu **Batch Y** của **Product Z**.
- **Ràng buộc**: Unique constraint giữa `(BinId, BatchId)`.
- **Trường tính toán `AvailableQty`**:
  $$\text{AvailableQty} = \text{Quantity} - \text{ReservedQty}$$
  - `Quantity`: Số lượng thực tế có trong ô.
  - `ReservedQty`: Số lượng đã được giữ chỗ bởi các phiếu GDN đang chờ xuất.

### 5.3 `StockTransaction` (`BusinessLayer.Entities.Stock.StockTransaction`)
- **Tác dụng & Vai trò**: Sổ cái bất biến (Append-only Ledger) ghi nhận MỌI biến động nhập, xuất, chuyển kho, điều chỉnh.
- **Đặc điểm**:
  - Không bao giờ bị UPDATE hoặc DELETE.
  - Lưu `QtyBefore` (số lượng trước biến động) và `QtyAfter` (số lượng sau biến động).
  - Khóa chính `Id` dạng `bigint` (long) để đáp ứng khối lượng dữ liệu lịch sử lớn.

---

## 6. Nhóm Mua hàng & Chứng từ Nhập kho (Purchase Order & GRN)

### 6.1 `PurchaseOrder` & `POLine` (`BusinessLayer.Entities.Orders`)
- **Tác dụng & Vai trò**: Đơn đặt mua hàng (`PO`) gửi cho Nhà cung cấp. `POLine` lưu danh sách sản phẩm và số lượng cần mua (`OrderedQty`) và số lượng đã giao (`ReceivedQty`).

### 6.2 `GoodsReceiptNote` (GRN) & `GRNLine` (`BusinessLayer.Entities.Orders`)
- **Tác dụng & Vai trò**: Phiếu nhập kho hàng hóa thực tế.
- **Quy trình**: Nhập dữ liệu DRAFT $\rightarrow$ Submit duyệt L1/L2 $\rightarrow$ Sau khi APPROVED, hệ thống tự động tạo `Batch`, tăng `BinStock` và ghi `StockTransaction`.
- **Hỗ trợ Trả hàng**: Trường `IsReturn` và `ParentGRNId` cho phép tạo phiếu Trả hàng nhập về cho NCC.

---

## 7. Nhóm Xuất kho & Chuyển kho (Dispatch & Transfer)

### 7.1 `DispatchRequest` (`BusinessLayer.Entities.Orders`)
- **Tác dụng & Vai trò**: Yêu cầu xuất hàng gửi từ bộ phận Sales hoặc Khách hàng.

### 7.2 `GoodsDispatchNote` (GDN) & `GDNLine` (`BusinessLayer.Entities.Orders`)
- **Tác dụng & Vai trò**: Phiếu xuất kho hàng hóa.
- **Quy trình**: DRAFT $\rightarrow$ Submit duyệt $\rightarrow$ APPROVED $\rightarrow$ PICKING $\rightarrow$ PICKED $\rightarrow$ DELIVERED.
- **Thuật toán FEFO**: Tự động gợi ý các Lô (`BatchId`) có hạn sử dụng gần nhất và vị trí `BinId` tối ưu để nhân viên lấy hàng.

### 7.3 `TransferOrder` & `TransferOrderLine` (`BusinessLayer.Entities.Orders`)
- **Tác dụng & Vai trò**: Phiếu điều chuyển hàng hóa nội bộ giữa các vị trí (từ `FromBinId` sang `ToBinId`) trong cùng một Kho.

---

## 8. Nhóm Kiểm kê & Điều chỉnh kho (Stock Count & Adjustment)

### 8.1 `StockCount` & `StockCountLine` (`BusinessLayer.Entities.Orders`)
- **Tác dụng & Vai trò**: Kế hoạch và Biên bản kiểm kê hàng tồn kho (Đột xuất, Định kỳ, Theo Zone/Rack).
- **Cơ chế**: Hệ thống chụp nhanh số lượng sổ sách (`SystemQty`). Nhân viên nhập số lượng thực tế đếm được (`ActualQty`). Hệ thống tự động tính chênh lệch (`Variance = ActualQty - SystemQty`).

### 8.2 `StockAdjustment` & `StockAdjustmentLine` (`BusinessLayer.Entities.Orders`)
- **Tác dụng & Vai trò**: Biên bản điều chỉnh số lượng tồn kho phát sinh sau đợt kiểm kê. Sau khi được duyệt, hệ thống cân bằng lại `BinStock` và ghi lịch sử vào `StockTransaction`.

---

## 9. Nhóm Động cơ Phê duyệt & Hệ thống (Approval & System)

### 9.1 `ApprovalWorkflow` (`BusinessLayer.Entities.Approvals`)
- **Tác dụng & Vai trò**: Cấu hình quy định cấp duyệt cho từng loại chứng từ (GRN, GDN, Transfer, Adjustment). Ví dụ: L1 = Trưởng kho, L2 = Giám đốc.

### 9.2 `Approval` (`BusinessLayer.Entities.Approvals`)
- **Tác dụng & Vai trò**: Lưu vết lịch sử phê duyệt **Đa hình (Polymorphic)**. Sử dụng cặp `(DocumentType, DocumentId)` để dùng chung 1 bảng duy nhất cho tất cả các loại chứng từ.

### 9.3 `Notification` (`BusinessLayer.Entities.System`)
- **Tác dụng & Vai trò**: Lưu trữ thông báo trong ứng dụng (In-app notifications) gửi tới người dùng (Cảnh báo hàng sắp hết hạn, tồn dưới định mức, nhắc duyệt phiếu).

### 9.4 `EmailLog` (`BusinessLayer.Entities.System`)
- **Tác dụng & Vai trò**: Nhật ký gửi email tự động (SMTP) của hệ thống, theo dõi trạng thái `QUEUED`, `SENT`, `FAILED` và số lần thử lại (`RetryCount`).

### 9.5 `SystemSetting` (`BusinessLayer.Entities.System`)
- **Tác dụng & Vai trò**: Lưu trữ các tham số cấu hình tĩnh của hệ thống dưới dạng Key-Value (Ví dụ: Email SMTP config, số ngày cảnh báo hết hạn mặc định...).
