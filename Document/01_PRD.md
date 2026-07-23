# Product Requirements Document (PRD)
# Warehouse Import-Export Management System (WMS)
# Hệ thống Quản lý Xuất Nhập Kho Logistics

---

**Version:** 1.0  
**Date:** 2025-06-19  
**Status:** Draft — Pending Review  
**Prepared by:** Senior Business Analyst  
**Stack:** ASP.NET Web (Frontend) · ASP.NET Web API (Backend) · SQL Server  

---

## Table of Contents
1. Business Overview
2. Problem Statement
3. Project Scope
4. User Roles & Permissions
5. Functional Requirements
6. Non-Functional Requirements
7. Use Cases
8. Business Process Flows
9. Database Design Suggestion
10. API Suggestion
11. UI Screen List
12. Dashboard & Reports
13. Integration Requirements
14. Security Requirements
15. Acceptance Criteria
16. Development Roadmap
17. Q&A Log

---

## 1. Business Overview

### 1.1 Company Context
Doanh nghiệp vận hành kho **Logistics đơn vị** (Single Tenant), cung cấp dịch vụ lưu kho và giao nhận hàng hóa cho khách hàng doanh nghiệp B2B và xử lý các đơn xuất hàng đến người nhận cuối (consignee). Quy mô vận hành hiện tại ở mức nhỏ đến trung bình: tối đa 5 kho, dưới 50 người dùng nội bộ, dưới 5.000 SKU và khoảng 200 giao dịch xuất nhập/ngày.

### 1.2 System Name
| Field | Value |
|---|---|
| **English Name** | Warehouse Import-Export Management System |
| **Vietnamese Name** | Hệ thống Quản lý Xuất Nhập Kho |
| **Abbreviation** | WMS |
| **Deployment Model** | Single Tenant |
| **Warehouse Type** | Kho Logistics |

### 1.3 Objectives
- Số hóa toàn bộ quy trình nhập kho, xuất kho, chuyển vị trí, kiểm kê.
- Kiểm soát tồn kho theo thời gian thực với nguyên tắc **FEFO** (First Expired, First Out).
- Chuẩn hóa luồng phê duyệt 2 cấp: Warehouse Manager → Giám đốc.
- Cung cấp dashboard và báo cáo vận hành cho Ban Giám Đốc.
- Tích hợp cảnh báo qua Email.
- Giao diện song ngữ Việt – Anh, responsive web.

---

## 2. Problem Statement

### 2.1 Current Pain Points
| # | Vấn đề hiện tại | Hệ quả |
|---|---|---|
| 1 | Quản lý tồn kho thủ công (Excel/giấy) | Sai lệch tồn kho, mất hàng không rõ nguyên nhân |
| 2 | Không có quản lý lô hàng / hạn sử dụng | Hàng tồn quá hạn không được xuất trước, phát sinh hàng hỏng |
| 3 | Phiếu nhập/xuất không được phê duyệt chính thức | Dữ liệu dễ bị thao túng, thiếu kiểm soát |
| 4 | Không có vị trí lưu trữ chi tiết (Rack/Shelf/Bin) | Mất thời gian tìm hàng, sai vị trí |
| 5 | Không cảnh báo tồn kho thấp hoặc hàng sắp hết hạn | Hết hàng đột ngột hoặc hàng quá hạn phát hiện muộn |
| 6 | Báo cáo tổng hợp phải làm thủ công | Tốn thời gian, dễ sai, không realtime |

### 2.2 Root Cause
Thiếu một hệ thống WMS tập trung, tích hợp quy trình từ nhập kho đến xuất kho, quản lý vị trí lưu trữ và hàng tồn theo lô, hạn sử dụng.

---

## 3. Project Scope

### 3.1 In Scope
- Quản lý danh mục: hàng hóa (SKU), nhà cung cấp, khách hàng, cấu trúc kho.
- Quy trình nhập kho (GRN – Goods Receipt Note).
- Quy trình xuất kho (GDN – Goods Dispatch Note).
- Quy trình chuyển vị trí trong cùng kho (Internal Transfer).
- Kiểm kê: Full Count, Periodic Count, Cycle Count.
- Quản lý Batch/Lot, ngày sản xuất, hạn sử dụng, FEFO.
- Phê duyệt 2 cấp cho mọi phiếu.
- Chứng từ: phiếu nhập, xuất, chuyển kho, kiểm kê, điều chỉnh tồn kho, trả NCC, trả KH.
- Cảnh báo qua Email và thông báo in-app.
- Báo cáo đầy đủ + Export Excel/PDF.
- RBAC phân quyền 7 vai trò.
- Giao diện song ngữ Việt – Anh, responsive web.

### 3.2 Out of Scope (Phase 1)
- Multi-tenant (nhiều doanh nghiệp).
- Chuyển hàng giữa các kho khác nhau (multi-warehouse transfer).
- Tích hợp ERP, SAP, phần mềm kế toán.
- Mobile native app (iOS/Android).
- Barcode/QR scanner hardware integration.
- SSO, MFA, Dark Mode.

---

## 4. User Roles & Permissions

### 4.1 Role Definitions
| Role | Tên tiếng Việt | Mô tả |
|---|---|---|
| **SYS_ADMIN** | Quản trị hệ thống | Toàn quyền cấu hình hệ thống, quản lý user |
| **DIRECTOR** | Giám đốc | Xem báo cáo, duyệt cấp 2 (cấp cuối) |
| **WH_MANAGER** | Trưởng kho | Duyệt cấp 1, phân công nhân viên, cấu hình kho |
| **WH_STAFF** | Nhân viên kho | Tạo phiếu nhập/xuất/chuyển kho, thực hiện thao tác kho |
| **INV_CTRL** | Kiểm soát tồn kho | Lập kế hoạch và thực hiện kiểm kê, điều chỉnh tồn kho |
| **PURCHASING** | Nhân viên mua hàng | Quản lý nhà cung cấp, tạo PO, theo dõi nhập hàng |
| **SALES** | Nhân viên kinh doanh | Quản lý khách hàng, tạo yêu cầu xuất kho |

### 4.2 Permission Matrix
| Feature Module | SYS_ADMIN | DIRECTOR | WH_MANAGER | WH_STAFF | INV_CTRL | PURCHASING | SALES |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| User Management | CRUD | - | View | - | - | - | - |
| System Config | CRUD | - | View | - | - | - | - |
| Warehouse Config | CRUD | View | CRUD | View | View | - | - |
| Product Catalog | CRUD | View | CRUD | View | View | CRUD | View |
| Supplier Mgmt | CRUD | View | View | - | - | CRUD | View |
| Customer Mgmt | CRUD | View | View | - | - | View | CRUD |
| GRN – Create | Yes | - | Yes | Yes | - | Yes | - |
| GRN – Approve L1 | Yes | - | Yes | - | - | - | - |
| GRN – Approve L2 | Yes | Yes | - | - | - | - | - |
| GDN – Create | Yes | - | Yes | Yes | - | - | Yes |
| GDN – Approve L1 | Yes | - | Yes | - | - | - | - |
| GDN – Approve L2 | Yes | Yes | - | - | - | - | - |
| Transfer – Create | Yes | - | Yes | Yes | Yes | - | - |
| Transfer – Approve | Yes | Yes | Yes | - | - | - | - |
| Inventory Count | Yes | View | Yes | Yes | Yes | - | - |
| Stock Adjustment | Yes | - | Yes | - | Yes | - | - |
| Reports | Yes | Yes | Yes | Limited | Yes | Limited | Limited |
| Export Excel/PDF | Yes | Yes | Yes | - | Yes | Yes | Yes |

---

## 5. Functional Requirements

### 5.1 FR-01: Quản lý Danh mục Hệ thống

#### FR-01.1: Quản lý Kho & Vị trí
| ID | Yêu cầu |
|---|---|
| FR-01.1.1 | Hệ thống quản lý cấu trúc kho 5 cấp: Warehouse → Zone → Rack → Shelf → Bin |
| FR-01.1.2 | Mỗi cấp có Mã, Tên, Mô tả, Trạng thái (Active/Inactive) |
| FR-01.1.3 | Một Bin chứa nhiều lô hàng (Batch) khác nhau |
| FR-01.1.4 | Một SKU có thể tồn tại ở nhiều Bin khác nhau |
| FR-01.1.5 | Hệ thống hiển thị sơ đồ cây (tree view) cấu trúc kho |

#### FR-01.2: Quản lý Hàng hóa (SKU/Product)
| ID | Yêu cầu |
|---|---|
| FR-01.2.1 | Mỗi sản phẩm có: Mã SKU, Tên (VI/EN), Danh mục, Đơn vị tính, Mô tả, Ảnh, Barcode |
| FR-01.2.2 | Thuộc tính lưu kho: Ngưỡng tồn thấp (Min Stock), Ngưỡng đặt hàng (Reorder Point) |
| FR-01.2.3 | Thuộc tính Batch: có/không quản lý Batch, có/không quản lý Expiry |
| FR-01.2.4 | Phân loại sản phẩm theo danh mục (Category) và nhóm (Group) |
| FR-01.2.5 | Tìm kiếm, lọc sản phẩm theo mã, tên, danh mục, trạng thái |

#### FR-01.3: Quản lý Nhà cung cấp
| ID | Yêu cầu |
|---|---|
| FR-01.3.1 | Thông tin: Mã NCC, Tên, MST, Địa chỉ, Email, Điện thoại, Website |
| FR-01.3.2 | Thông tin hợp đồng: Số hợp đồng, Ngày ký, Ngày hết hạn, Điều khoản thanh toán |
| FR-01.3.3 | Lịch sử giao dịch nhập hàng theo NCC |
| FR-01.3.4 | Trạng thái: Active / Inactive / Blacklisted |

#### FR-01.4: Quản lý Khách hàng
| ID | Yêu cầu |
|---|---|
| FR-01.4.1 | Phân loại KH: (1) Khách thuê dịch vụ lưu kho B2B, (2) Consignee nhận hàng theo đơn |
| FR-01.4.2 | Thông tin: Mã KH, Tên, MST, Địa chỉ, Email, Điện thoại, Loại KH |
| FR-01.4.3 | Với KH B2B: thông tin hợp đồng dịch vụ lưu kho |
| FR-01.4.4 | Lịch sử giao dịch xuất hàng theo KH |

### 5.2 FR-02: Quản lý Lô hàng (Batch/Lot)
| ID | Yêu cầu |
|---|---|
| FR-02.1 | Mỗi Batch gắn với: SKU, Số lô (Lot Number), Ngày sản xuất (MFG Date), Hạn sử dụng (Expiry Date), Nhà cung cấp, Số lượng nhập gốc |
| FR-02.2 | Hệ thống tự động đề xuất xuất theo nguyên tắc **FEFO** (hạn gần nhất xuất trước) |
| FR-02.3 | Cảnh báo lô hàng sắp hết hạn trước N ngày (cấu hình được) |
| FR-02.4 | Cảnh báo lô hàng đã hết hạn, highlight đỏ trên giao diện |
| FR-02.5 | Xem toàn bộ lịch sử vận chuyển của một Batch (trace history) |

### 5.3 FR-03: Quy trình Nhập kho (GRN)
| ID | Yêu cầu |
|---|---|
| FR-03.1 | Tạo PO (Purchase Order): PURCHASING tạo đơn đặt hàng với NCC |
| FR-03.2 | Tạo phiếu GRN: WH_STAFF/WH_MANAGER tạo phiếu nhập kho liên kết PO |
| FR-03.3 | Phiếu GRN gồm: Số phiếu (auto), ngày, NCC, danh sách hàng + lô + số lượng + vị trí lưu |
| FR-03.4 | Duyệt Cấp 1 (L1): WH_MANAGER xem xét và phê duyệt hoặc từ chối |
| FR-03.5 | Duyệt Cấp 2 (L2): DIRECTOR phê duyệt cuối hoặc từ chối |
| FR-03.6 | Khi DIRECTOR phê duyệt → hệ thống tự động cập nhật tồn kho tại Bin được chỉ định |
| FR-03.7 | Gửi Email thông báo kết quả duyệt cho người tạo phiếu |
| FR-03.8 | Có thể đính kèm chứng từ (PDF, ảnh) vào phiếu GRN |
| FR-03.9 | Người tạo phiếu có thể hủy phiếu khi còn ở trạng thái DRAFT hoặc bị từ chối |
| FR-03.10 | Phiếu trả hàng NCC (Return to Supplier): reverse quy trình GRN, trừ tồn kho |

**Trạng thái GRN:** `DRAFT → PENDING_L1 → PENDING_L2 → APPROVED / REJECTED / CANCELLED`

### 5.4 FR-04: Quy trình Xuất kho (GDN)
| ID | Yêu cầu |
|---|---|
| FR-04.1 | Tạo yêu cầu xuất: SALES/WH_STAFF tạo yêu cầu xuất kho cho KH |
| FR-04.2 | Tạo phiếu GDN: liên kết yêu cầu xuất, chọn hàng + Batch (FEFO đề xuất tự động) |
| FR-04.3 | Phiếu GDN gồm: Số phiếu (auto), ngày, KH/Consignee, danh sách hàng + Batch + số lượng + vị trí xuất |
| FR-04.4 | Duyệt L1 (WH_MANAGER) → Duyệt L2 (DIRECTOR) |
| FR-04.5 | Sau khi DIRECTOR duyệt → trạng thái APPROVED, cho phép Picking |
| FR-04.6 | Picking: WH_STAFF xác nhận đã lấy hàng thực tế tại Bin |
| FR-04.7 | Sau Picking → trạng thái PICKED, WH_STAFF tạo Xác nhận giao hàng |
| FR-04.8 | Xác nhận giao hàng → trạng thái DELIVERED → cập nhật tồn kho (trừ tồn) |
| FR-04.9 | Gửi Email thông báo kết quả duyệt và xác nhận giao hàng |
| FR-04.10 | Phiếu trả hàng từ KH (Customer Return): cộng lại tồn kho, tạo lô mới hoặc nhập vào lô cũ |

**Trạng thái GDN:** `DRAFT → PENDING_L1 → PENDING_L2 → APPROVED → PICKING → PICKED → DELIVERED / CANCELLED`

### 5.5 FR-05: Quy trình Chuyển vị trí (Internal Transfer)
| ID | Yêu cầu |
|---|---|
| FR-05.1 | Cho phép chuyển hàng từ Bin/Shelf/Rack/Zone này sang vị trí khác trong cùng kho |
| FR-05.2 | Phiếu chuyển kho: Số phiếu, ngày, hàng + Batch + số lượng, Vị trí nguồn, Vị trí đích |
| FR-05.3 | Quy trình duyệt: WH_STAFF tạo → WH_MANAGER duyệt → DIRECTOR duyệt |
| FR-05.4 | Sau duyệt → cập nhật vị trí lưu trữ trong hệ thống |
| FR-05.5 | Lịch sử chuyển vị trí lưu lại đầy đủ |

### 5.6 FR-06: Kiểm kê Tồn kho
| ID | Yêu cầu |
|---|---|
| FR-06.1 | Full Count: Kiểm kê toàn bộ SKU trong kho tại một thời điểm |
| FR-06.2 | Periodic Count: Kiểm kê theo chu kỳ (tuần/tháng/quý) cho toàn kho hoặc một Zone |
| FR-06.3 | Cycle Count: Kiểm kê vòng xoay theo Zone/Rack, không dừng hoạt động kho |
| FR-06.4 | INV_CTRL lập kế hoạch kiểm kê → giao phiếu cho WH_STAFF thực hiện |
| FR-06.5 | WH_STAFF nhập số lượng thực tế đếm được vào hệ thống |
| FR-06.6 | Hệ thống tự động so sánh số lượng hệ thống vs thực tế, highlight chênh lệch |
| FR-06.7 | INV_CTRL/WH_MANAGER xem xét chênh lệch và tạo Biên bản điều chỉnh tồn kho |
| FR-06.8 | DIRECTOR phê duyệt biên bản điều chỉnh → cập nhật tồn kho chính thức |
| FR-06.9 | Cảnh báo Email khi chênh lệch kiểm kê vượt ngưỡng (% hoặc số lượng, cấu hình được) |

### 5.7 FR-07: Cảnh báo & Thông báo
| ID | Trigger | Kênh | Người nhận |
|---|---|---|---|
| FR-07.1 | Tồn kho SKU dưới ngưỡng Min Stock | Email + In-app | WH_MANAGER, PURCHASING |
| FR-07.2 | Lô hàng sắp hết hạn (N ngày, cấu hình được) | Email + In-app | WH_MANAGER, INV_CTRL |
| FR-07.3 | Lô hàng đã hết hạn | Email + In-app | WH_MANAGER, INV_CTRL |
| FR-07.4 | Phiếu GRN/GDN/Transfer đang chờ duyệt L1 | Email + In-app | WH_MANAGER |
| FR-07.5 | Phiếu đang chờ duyệt L2 | Email + In-app | DIRECTOR |
| FR-07.6 | Phiếu bị từ chối | Email + In-app | Người tạo phiếu |
| FR-07.7 | Chênh lệch kiểm kê vượt ngưỡng | Email + In-app | WH_MANAGER, INV_CTRL |
| FR-07.8 | Hàng xuất xác nhận giao thành công | Email | SALES, KH (nếu có email KH) |

### 5.8 FR-08: Báo cáo & Thống kê
| ID | Tên báo cáo | Mô tả |
|---|---|---|
| FR-08.1 | Tồn kho hiện tại | Tồn kho real-time theo SKU, Batch, Vị trí |
| FR-08.2 | Nhập – Xuất – Tồn theo kỳ | So sánh nhập/xuất/tồn theo ngày/tuần/tháng |
| FR-08.3 | Hàng sắp hết tồn | Danh sách SKU dưới ngưỡng Min Stock |
| FR-08.4 | Hàng sắp hết hạn / đã hết hạn | Danh sách Batch theo Expiry Date |
| FR-08.5 | Lịch sử giao dịch chi tiết | Toàn bộ phiếu nhập/xuất/chuyển kho theo bộ lọc |
| FR-08.6 | Hiệu suất kho | Thời gian xử lý trung bình từ tạo phiếu → hoàn thành |
| FR-08.7 | Theo nhà cung cấp | Tổng nhập hàng, giá trị, tần suất theo từng NCC |
| FR-08.8 | Theo khách hàng | Tổng xuất hàng, tần suất theo từng KH |
| FR-08.9 | Chênh lệch kiểm kê | Lịch sử và xu hướng chênh lệch theo SKU/Zone |
| FR-08.10 | Export | Tất cả báo cáo xuất được Excel và PDF |

---

## 6. Non-Functional Requirements

### 6.1 Performance
| ID | Yêu cầu |
|---|---|
| NFR-01 | API response time < 2 giây cho 95% requests (dưới tải bình thường) |
| NFR-02 | Dashboard load time < 3 giây |
| NFR-03 | Hỗ trợ tối đa 50 concurrent users |
| NFR-04 | Export báo cáo lớn (>10.000 dòng) hoàn thành trong < 30 giây |

### 6.2 Availability
| ID | Yêu cầu |
|---|---|
| NFR-05 | Uptime tối thiểu 99% (không kể maintenance window) |
| NFR-06 | Backup database tự động hàng ngày |
| NFR-07 | Thời gian phục hồi (RTO) < 4 giờ sau sự cố |

### 6.3 Security
| ID | Yêu cầu |
|---|---|
| NFR-08 | RBAC – phân quyền chi tiết theo vai trò |
| NFR-09 | Tất cả API phải xác thực JWT token |
| NFR-10 | Password hash bằng BCrypt, độ phức tạp tối thiểu |
| NFR-11 | HTTPS bắt buộc cho toàn bộ traffic |
| NFR-12 | Session timeout sau 8 giờ không hoạt động |
| NFR-13 | Không lưu thông tin nhạy cảm trong localStorage |

### 6.4 Usability
| ID | Yêu cầu |
|---|---|
| NFR-14 | Responsive Web: tương thích desktop (1920x1080), tablet (1024x768), mobile browser |
| NFR-15 | Hỗ trợ chuyển đổi ngôn ngữ Việt ↔ Anh tức thì, không cần reload |
| NFR-16 | Giao diện Enterprise Dashboard: sidebar navigation, breadcrumb, table pagination |
| NFR-17 | Tất cả form có validation rõ ràng, thông báo lỗi tiếng Việt và tiếng Anh |

### 6.5 Maintainability
| ID | Yêu cầu |
|---|---|
| NFR-18 | Code coverage unit test tối thiểu 60% cho Business Logic layer |
| NFR-19 | API documentation bằng Swagger/OpenAPI |
| NFR-20 | Log lỗi tập trung (Application Event Log hoặc file log) |

---

## 7. Use Cases

### UC-01: Nhập kho (GRN)
**Actor:** WH_STAFF (tạo), WH_MANAGER (duyệt L1), DIRECTOR (duyệt L2)  
**Precondition:** Sản phẩm, NCC, vị trí kho đã tồn tại trong hệ thống  

**Main Flow:**
1. WH_STAFF tạo phiếu GRN, nhập thông tin hàng + batch + vị trí
2. Submit → trạng thái PENDING_L1, Email gửi WH_MANAGER
3. WH_MANAGER review → Approve → PENDING_L2, Email gửi DIRECTOR
4. DIRECTOR Approve → APPROVED → Tồn kho tự động cập nhật
5. Email xác nhận gửi người tạo phiếu

**Alternative Flow:**
- Bước 3/4: Từ chối → REJECTED, Email thông báo + lý do từ chối
- Bước 1: Hủy phiếu DRAFT → CANCELLED

---

### UC-02: Xuất kho (GDN) theo FEFO
**Actor:** SALES (tạo yêu cầu), WH_STAFF (tạo phiếu, picking), WH_MANAGER (duyệt L1), DIRECTOR (duyệt L2)  
**Precondition:** KH tồn tại, hàng có tồn kho đủ, có Batch hợp lệ  

**Main Flow:**
1. SALES tạo yêu cầu xuất kho cho KH
2. WH_STAFF tạo phiếu GDN, hệ thống tự đề xuất Batch theo FEFO
3. WH_STAFF xác nhận chọn Batch → Submit
4. WH_MANAGER duyệt L1 → DIRECTOR duyệt L2 → APPROVED
5. WH_STAFF thực hiện Picking tại Bin
6. WH_STAFF xác nhận giao hàng → DELIVERED → Trừ tồn kho

---

### UC-03: Kiểm kê Cycle Count
**Actor:** INV_CTRL (lập kế hoạch), WH_STAFF (thực hiện), INV_CTRL/WH_MANAGER (review), DIRECTOR (duyệt điều chỉnh)  

**Main Flow:**
1. INV_CTRL tạo kế hoạch Cycle Count cho Zone X
2. Giao phiếu kiểm kê cho WH_STAFF
3. WH_STAFF đếm thực tế và nhập vào hệ thống
4. Hệ thống so sánh, highlight chênh lệch
5. Nếu chênh lệch vượt ngưỡng → Email cảnh báo WH_MANAGER
6. INV_CTRL tạo Biên bản điều chỉnh
7. DIRECTOR duyệt → Cập nhật tồn kho

---

### UC-04: Cảnh báo hàng sắp hết hạn
**Actor:** Hệ thống (scheduler tự động)  

**Main Flow:**
1. Scheduler chạy mỗi ngày lúc 07:00
2. Quét toàn bộ Batch có Expiry Date trong vòng N ngày
3. Gửi Email danh sách hàng sắp hết hạn đến WH_MANAGER, INV_CTRL
4. Cập nhật In-app notification badge

---

## 8. Business Process Flows

### 8.1 Quy trình Nhập kho (GRN)
```
[PURCHASING] Tao PO
      |
      v
[WH_STAFF] Hang ve -> Tao phieu GRN (DRAFT)
  - Nhap thong tin lo hang (Batch/Lot)
  - Chon vi tri luu (Bin)
  - Dinh kem chung tu
      |
Submit -> Status: PENDING_L1
      |
[EMAIL] -> WH_MANAGER
      |
[WH_MANAGER] Review phieu GRN
   Tu choi <---------------- Approve
      |                          |
   REJECTED               Status: PENDING_L2
[EMAIL] nguoi tao       [EMAIL] DIRECTOR
                                 |
                     [DIRECTOR] Review
                 Tu choi <------ Approve
                     |               |
                 REJECTED         APPROVED
              [EMAIL]          Ton kho cap nhat
              nguoi tao       [EMAIL] nguoi tao
```

### 8.2 Quy trình Xuất kho (GDN)
```
[SALES] Yeu cau xuat kho cho KH
      |
      v
[WH_STAFF] Tao phieu GDN
  - He thong de xuat Batch theo FEFO
  - WH_STAFF xac nhan Batch, So luong, Vi tri
      |
Submit -> PENDING_L1 -> [WH_MANAGER duyet] -> PENDING_L2
      |
[DIRECTOR duyet] -> APPROVED
      |
[WH_STAFF] Picking (xac nhan lay hang tai Bin)
      |
Status: PICKED
      |
[WH_STAFF] Xac nhan giao hang cho KH/Consignee
      |
Status: DELIVERED -> Ton kho duoc tru
[EMAIL] SALES + KH (neu co email)
```

### 8.3 Quy trình Kiểm kê
```
[INV_CTRL] Len ke hoach kiem ke
  - Chon loai: Full / Periodic / Cycle
  - Chon pham vi: Toan kho / Zone / Rack
      |
Phat phieu kiem ke cho WH_STAFF
      |
[WH_STAFF] Dem thuc te -> Nhap so luong vao he thong
      |
[He thong] So sanh: Ton he thong vs Thuc te
      |
Chenh lech?
  Khong -> Hoan thanh kiem ke
  Co -> Email canh bao (neu vuot nguong)
       |
[INV_CTRL] Tao Bien ban dieu chinh ton kho
      |
[DIRECTOR] Duyet bien ban
      |
Ton kho duoc dieu chinh chinh thuc
```

---

## 9. Database Design Suggestion
*(Xem chi tiết đầy đủ tại file `02_ERD_CodeFirst_Models.md` và `03_Database_Schema.sql`)*

### 9.1 Core Entities
| Entity | Table | Mô tả |
|---|---|---|
| Warehouse | Warehouses | Kho lưu trữ |
| Zone | Zones | Khu vực trong kho |
| Rack | Racks | Kệ trong Zone |
| Shelf | Shelves | Tầng kệ |
| Bin | Bins | Ô lưu trữ (đơn vị nhỏ nhất) |
| Product | Products | Danh mục hàng hóa/SKU |
| ProductCategory | ProductCategories | Danh mục sản phẩm |
| Supplier | Suppliers | Nhà cung cấp |
| Customer | Customers | Khách hàng |
| Batch | Batches | Lô hàng có MFG/Expiry |
| BinStock | BinStocks | Tồn kho chi tiết theo Bin + Batch (bảng trung tâm) |
| PurchaseOrder | PurchaseOrders | Đơn đặt hàng NCC |
| GRN | GoodsReceiptNotes | Phiếu nhập kho |
| GRNLine | GRNLines | Chi tiết phiếu nhập |
| GDN | GoodsDispatchNotes | Phiếu xuất kho |
| GDNLine | GDNLines | Chi tiết phiếu xuất |
| Transfer | TransferOrders | Phiếu chuyển vị trí |
| TransferLine | TransferOrderLines | Chi tiết phiếu chuyển |
| StockCount | StockCounts | Phiên kiểm kê |
| StockCountLine | StockCountLines | Chi tiết kiểm kê từng Bin/SKU |
| StockAdjustment | StockAdjustments | Biên bản điều chỉnh tồn kho |
| Approval | Approvals | Lịch sử phê duyệt (polymorphic) |
| Notification | Notifications | Thông báo in-app |
| User | Users | Người dùng hệ thống |
| Role | Roles | Vai trò |

### 9.2 Key Relationships
- Products → ProductCategories (N:1)
- Batches → Products, Suppliers (N:1)
- BinStocks → Bins, Batches (N:1) — **Đây là bảng tồn kho chính**
- GRNLines → GRN, Products, Batches, Bins
- GDNLines → GDN, Products, Batches, Bins
- Approvals → GRN/GDN/Transfer (polymorphic qua DocumentType + DocumentId)

---

## 10. API Suggestion
*(Xem chi tiết đầy đủ tại file `04_API_List.md`)*

### 10.1 API Groups
| Group | Base Path | Mô tả |
|---|---|---|
| Auth | /api/auth | Đăng nhập, refresh token, đổi mật khẩu |
| Users | /api/users | CRUD người dùng, phân quyền |
| Warehouse Structure | /api/warehouses, /api/zones, /api/racks, /api/shelves, /api/bins | Cấu trúc kho |
| Products | /api/products, /api/categories | Danh mục hàng hóa |
| Suppliers | /api/suppliers | Nhà cung cấp |
| Customers | /api/customers | Khách hàng |
| Batches | /api/batches | Quản lý lô hàng |
| Stock | /api/stock | Tồn kho real-time, FEFO query |
| Purchase Orders | /api/purchase-orders | Đơn đặt hàng |
| GRN | /api/grn | Phiếu nhập kho |
| GDN | /api/gdn | Phiếu xuất kho |
| Transfers | /api/transfers | Phiếu chuyển vị trí |
| Stock Count | /api/stock-counts | Kiểm kê |
| Stock Adjustment | /api/stock-adjustments | Điều chỉnh tồn kho |
| Approvals | /api/approvals | Phê duyệt |
| Reports | /api/reports | Báo cáo & Export |
| Notifications | /api/notifications | Thông báo |
| Dashboard | /api/dashboard | Dữ liệu Dashboard |

---

## 11. UI Screen List
*(Xem chi tiết đầy đủ 60 màn hình tại file `06_UI_Screens.md`)*

Tóm tắt theo nhóm:
| Nhóm | Số màn hình | Ví dụ |
|---|---|---|
| Authentication | 3 | Login, Change Password, Forgot Password |
| Dashboard | 2 | Main Dashboard, Quick Reports |
| Cấu trúc kho | 4 | Warehouse List, Tree View, Zone/Rack/Bin |
| Sản phẩm | 4 | Product List, Form, Detail, Categories |
| NCC & KH | 4 | Supplier/Customer List & Form |
| Batch | 3 | Batch List, Detail, Expiring |
| GRN | 7 | PO, GRN List/Create/Detail, Returns |
| GDN | 8 | Dispatch Request, GDN List/Create/Detail, Picking, Delivery, Returns |
| Transfer | 3 | Transfer List/Create/Detail |
| Kiểm kê | 5 | Count Plan, Execution, Result, Adjustment |
| Tồn kho | 3 | Current Stock, By Location, By Batch |
| Phê duyệt | 2 | Approval Inbox, Detail |
| Báo cáo | 8 | 8 loại báo cáo với filter + export |
| Hệ thống | 4 | Users, Roles, Settings, Notifications |
| **Tổng** | **60** | |

---

## 12. Dashboard & Reports

### 12.1 Main Dashboard — Widgets
| Widget | Dữ liệu hiển thị | Role xem |
|---|---|---|
| KPI Cards | Tổng SKU, Tổng Batch, Giao dịch hôm nay, Cảnh báo tồn thấp | Tất cả |
| Biểu đồ Nhập-Xuất 7 ngày | Bar chart: nhập vs xuất | Manager, Director |
| Top 10 SKU Tồn thấp | Danh sách + % so với Min Stock | Manager, INV_CTRL |
| Phiếu chờ duyệt | Count + danh sách nhanh | Manager, Director |
| Batch sắp hết hạn (30 ngày) | Danh sách Batch + ngày còn lại | Manager, INV_CTRL |
| Hoạt động gần nhất | Timeline 10 giao dịch mới nhất | Staff+ |

### 12.2 Report Filters (Chung)
Tất cả báo cáo hỗ trợ filter: Khoảng thời gian, Kho, Zone, SKU, NCC, KH, Trạng thái

---

## 13. Integration Requirements

### 13.1 Email Integration
| ID | Yêu cầu |
|---|---|
| INT-01 | Cấu hình SMTP server trong System Settings |
| INT-02 | Email template HTML có logo, bảng dữ liệu rõ ràng |
| INT-03 | Hỗ trợ email template song ngữ (gửi theo ngôn ngữ người nhận) |
| INT-04 | Retry logic khi gửi email thất bại (3 lần, interval 5 phút) |
| INT-05 | Log lịch sử email đã gửi (timestamp, recipient, status) |

### 13.2 Scheduled Jobs
| Job | Lịch chạy | Chức năng |
|---|---|---|
| ExpiryAlertJob | Hàng ngày 07:00 | Quét Batch sắp hết hạn, gửi Email |
| LowStockAlertJob | Hàng ngày 07:30 | Quét tồn kho dưới ngưỡng, gửi Email |
| PendingApprovalReminderJob | Hàng ngày 08:00 | Nhắc nhở phiếu chờ duyệt quá 24h |

---

## 14. Security Requirements
| ID | Yêu cầu |
|---|---|
| SEC-01 | JWT Authentication với Access Token (1h) + Refresh Token (7 ngày) |
| SEC-02 | RBAC: mọi API endpoint kiểm tra role permission trước khi xử lý |
| SEC-03 | Password: tối thiểu 8 ký tự, có chữ hoa, chữ thường, số; hash BCrypt cost=12 |
| SEC-04 | HTTPS bắt buộc (TLS 1.2+); không cho phép HTTP |
| SEC-05 | CORS chỉ cho phép origin của frontend application |
| SEC-06 | Rate limiting: tối đa 100 requests/phút/IP cho API công khai |
| SEC-07 | SQL Injection prevention: dùng Entity Framework parameterized queries |
| SEC-08 | XSS prevention: sanitize input, Content Security Policy headers |
| SEC-09 | File upload: chỉ cho phép PDF, JPG, PNG; kiểm tra MIME type thực; giới hạn 10MB/file |
| SEC-10 | Session timeout: tự động logout sau 8 giờ không hoạt động |

---

## 15. Acceptance Criteria

### Module Nhập kho (GRN)
- [ ] Tạo phiếu GRN thành công, số phiếu tự động sinh theo format GRN-YYYYMM-XXXXX
- [ ] Phiếu chuyển trạng thái đúng theo luồng: DRAFT → PENDING_L1 → PENDING_L2 → APPROVED
- [ ] Email được gửi đúng người tại mỗi bước duyệt
- [ ] Tồn kho tăng chính xác sau khi DIRECTOR duyệt (kiểm tra BinStock)
- [ ] Phiếu trả NCC trừ đúng tồn kho theo Batch

### Module Xuất kho (GDN)
- [ ] FEFO hoạt động đúng: Batch có Expiry Date gần nhất luôn được đề xuất trước
- [ ] Tồn kho trừ chính xác sau bước DELIVERED
- [ ] Trạng thái GDN đúng: DRAFT → PENDING_L1 → PENDING_L2 → APPROVED → PICKING → PICKED → DELIVERED
- [ ] Không cho phép xuất quá số lượng tồn kho hiện tại của Batch

### Module Kiểm kê
- [ ] Cycle Count không ảnh hưởng đến hoạt động nhập/xuất trong cùng Zone
- [ ] Hệ thống tính đúng chênh lệch: Thực tế − Hệ thống (có thể âm hoặc dương)
- [ ] Biên bản điều chỉnh chỉ được tạo sau khi kiểm kê hoàn thành

### Dashboard & Reports
- [ ] Dữ liệu tồn kho trên Dashboard khớp với bảng BinStocks
- [ ] Export Excel chứa đúng dữ liệu theo filter đang áp dụng
- [ ] Export PDF có header, footer, logo, ngày xuất

### Bảo mật
- [ ] User không có role phù hợp nhận HTTP 403 khi gọi API
- [ ] JWT hết hạn nhận HTTP 401

---

## 16. Development Roadmap
*(Xem chi tiết đầy đủ tại file `07_Development_Plan.md`)*

### High-Level Milestones — 4 tuần / 2 thành viên
| Week | Milestone |
|---|---|
| Week 1 | Foundation: DB, Auth, Warehouse Structure, Product Catalog, Supplier/Customer |
| Week 2 | Core Transactions: GRN, GDN (FEFO), Approval Engine |
| Week 3 | Operations: Transfer, Stock Count, Stock Adjustment, Notifications |
| Week 4 | Reports, Dashboard, Export, Testing, Polish & Deploy |

---

## 17. Q&A Log
| # | Câu hỏi | Trả lời | Quyết định thiết kế |
|---|---|---|---|
| 1 | Deployment model? | Single Tenant | Không cần schema isolation, TenantId |
| 2 | Loại kho? | Kho Logistics | Hỗ trợ cả KH B2B lưu kho và Consignee nhận hàng |
| 3 | Ngôn ngữ? | Song ngữ VI-EN | i18n client-side, toggle không reload |
| 4 | Người dùng? | 7 vai trò | RBAC 7 roles, permission matrix chi tiết |
| 5 | Cấu trúc kho? | 5 cấp đầy đủ | Warehouse→Zone→Rack→Shelf→Bin |
| 6 | Quản lý hàng? | Batch/Lot Number | Bảng Batches riêng, liên kết BinStocks |
| 7 | Hạn sử dụng? | MFG + Expiry + FEFO | FEFO auto-suggest khi tạo GDN |
| 8 | Quy trình nhập? | Giữ nguyên đề xuất | PO → GRN → Duyệt L1/L2 → Cập nhật tồn |
| 9 | Quy trình xuất? | Giữ nguyên đề xuất | GDN → Duyệt L1/L2 → Picking → Delivered |
| 10 | Chuyển kho? | Chỉ trong cùng 1 kho | TransferOrder trong cùng WarehouseId |
| 11 | Kiểm kê? | Full + Periodic + Cycle | 3 loại, không cần đột xuất |
| 12 | NCC? | Đầy đủ thông tin | Suppliers table đầy đủ fields + hợp đồng |
| 13 | KH? | Cả 2 loại | CustomerType enum: B2B_SERVICE, CONSIGNEE |
| 14 | Chứng từ? | Đầy đủ 7 loại | Tất cả phiếu + Return GRN/GDN |
| 15 | Phê duyệt? | 2 cấp cố định mọi phiếu | ApprovalLevel enum: L1Manager, L2Director |
| 16 | Tích hợp? | Chỉ Email | SMTP config trong Settings, EmailLog table |
| 17 | Báo cáo? | Tất cả | 9 loại báo cáo + Dashboard |
| 18 | Cảnh báo? | Tất cả | 8 trigger, Email + In-app |
| 19 | Audit Log? | Không (before/after) | Chỉ lưu StatusHistory cho phiếu, không full audit |
| 20 | Quy mô? | ~5 kho, <50 user, <5K SKU | INT cho IDs, không cần bigint (trừ StockTransaction), page size 20-50 |
| 21 | Phi chức năng? | Responsive, RBAC, Export | Bootstrap responsive, EPPlus/QuestPDF Export |

---
*End of PRD v1.0*
