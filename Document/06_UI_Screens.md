# WMS — UI Screen Specifications
# Version 1.0

---

## Design System

- **Framework:** Bootstrap 5.3 (custom Enterprise theme)
- **Primary Color:** #1A3C5E (dark navy)
- **Accent Color:** #2E8BC0
- **Warning:** #F59E0B | **Danger:** #EF4444 | **Success:** #22C55E
- **Font:** Inter (main)
- **Layout:** Fixed sidebar 250px + top header 60px + main content

---

## Global Components

| Component | Description |
|---|---|
| Top Header | Logo, User info, Language toggle VI/EN, Notification bell (badge) |
| Sidebar Navigation | Icon + text, collapsible groups |
| Breadcrumb | Current location path |
| Toast Notifications | Pop-up corner for success/error/warning |
| Confirm Dialog | Modal before irreversible actions |

---

## Screen Inventory (60 screens total)

### AUTH (S-01 to S-03)
| Screen | Route | Key Elements |
|---|---|---|
| S-01 Login | /login | Username, Password, Language toggle, Forgot password link |
| S-02 Change Password | /change-password | Old pass, New pass, Strength meter |
| S-03 Forgot Password | /forgot-password | Email input, Send reset link |

### DASHBOARD (S-04 to S-05)
| Screen | Route | Key Elements |
|---|---|---|
| S-04 Main Dashboard | /dashboard | 4 KPI cards, Nhap-Xuat chart (7 days), Pending approvals, Low stock top 10, Expiring batches, Activity timeline |
| S-05 Quick Reports | /dashboard/quick-reports | Shortcuts to key reports |

### WAREHOUSE STRUCTURE (S-06 to S-09)
| Screen | Route | Key Elements |
|---|---|---|
| S-06 Warehouses List | /warehouses | Table + Add button |
| S-07 Warehouse Tree | /warehouses/:id/structure | 5-level expandable tree + detail panel |
| S-08 Zone Management | /warehouses/:id/zones | CRUD zones |
| S-09 Rack/Shelf/Bin | /zones/:id/locations | Nested CRUD |

### PRODUCTS (S-10 to S-13)
| Screen | Route | Key Elements |
|---|---|---|
| S-10 Product List | /products | Filterable table, low stock highlight |
| S-11 Product Form | /products/new, /products/:id/edit | Basic info, stock thresholds, batch config, image upload |
| S-12 Product Detail | /products/:id | Info tabs + stock by batch/bin + transaction history |
| S-13 Categories | /product-categories | Tree CRUD |

### SUPPLIERS & CUSTOMERS (S-14 to S-17)
| Screen | Route | Key Elements |
|---|---|---|
| S-14 Supplier List | /suppliers | Status badges, contract expiry warning |
| S-15 Supplier Form | /suppliers/new, /:id/edit | 2-tab: Info + Contract |
| S-16 Customer List | /customers | Type badge (B2B_SERVICE vs CONSIGNEE) |
| S-17 Customer Form | /customers/new, /:id/edit | Type selector, contract for B2B |

### BATCH MANAGEMENT (S-18 to S-20)
| Screen | Route | Key Elements |
|---|---|---|
| S-18 Batch List | /batches | Quick filter tabs: All/Expiring/Expired; color rows by expiry |
| S-19 Batch Detail | /batches/:id | Countdown timer, stock by bin, movement history |
| S-20 Expiring Batches | /batches/expiring | Focused view for upcoming expirations |

### GRN - GOODS RECEIPT (S-21 to S-27)
| Screen | Route | Key Elements |
|---|---|---|
| S-21 PO List | /purchase-orders | Status filter, link to create GRN |
| S-22 Create PO | /purchase-orders/new | Supplier, items, expected date |
| S-23 PO Detail | /purchase-orders/:id | Lines + receipt progress |
| S-24 GRN List | /grn | Status tabs (Pending L1/L2/Approved/Rejected) |
| S-25 Create GRN | /grn/new | 2-panel: Header form + dynamic lines with Bin selector |
| S-26 GRN Detail | /grn/:id | Approval vertical timeline stepper |
| S-27 GRN Returns | /grn/returns | Return to supplier form |

### GDN - GOODS DISPATCH (S-28 to S-35)
| Screen | Route | Key Elements |
|---|---|---|
| S-28 Dispatch Requests List | /dispatch-requests | Status filter |
| S-29 Create Dispatch Request | /dispatch-requests/new | Customer, items needed |
| S-30 GDN List | /gdn | Status tabs including PICKING/PICKED/DELIVERED |
| S-31 Create GDN | /gdn/new | FEFO suggestion panel per line |
| S-32 GDN Detail | /gdn/:id | Full approval + delivery timeline |
| S-33 Picking Screen | /gdn/:id/picking | Large font location, checkbox per line, progress bar |
| S-34 Delivery Confirm | /gdn/:id/delivery | Final qty confirmation + delivery note |
| S-35 Customer Returns | /gdn/returns | Return from customer form |

### TRANSFERS (S-36 to S-38)
| Screen | Route | Key Elements |
|---|---|---|
| S-36 Transfer List | /transfers | Status filter |
| S-37 Create Transfer | /transfers/new | From Bin -> To Bin selector, batch select |
| S-38 Transfer Detail | /transfers/:id | Lines + approval timeline |

### STOCK COUNT (S-39 to S-43)
| Screen | Route | Key Elements |
|---|---|---|
| S-39 Count Plan List | /stock-counts | Type badges: FULL/PERIODIC/CYCLE |
| S-40 Create Count Plan | /stock-counts/new | Type, scope (zone/rack), date, assign staff |
| S-41 Count Execution | /stock-counts/:id/count | Actual qty input per Bin/Batch; auto-save |
| S-42 Count Result | /stock-counts/:id/result | Variance table (color: red negative, green positive) |
| S-43 Stock Adjustments | /stock-adjustments | Adjustment list + create adjustment form |

### INVENTORY VIEW (S-44 to S-46)
| Screen | Route | Key Elements |
|---|---|---|
| S-44 Current Stock | /stock | SKU-level stock with Min Stock indicator icons |
| S-45 Stock by Location | /stock/by-location | Tree nav + Rack grid visualization |
| S-46 Stock by Batch | /stock/by-batch | Batch table with expiry sorting |

### APPROVALS (S-47 to S-48)
| Screen | Route | Key Elements |
|---|---|---|
| S-47 Approval Inbox | /approvals | Pending/Approved/Rejected tabs; bulk approve option |
| S-48 Approval Detail | /approvals/:id | Summary + preview lines + Approve/Reject buttons + history |

### REPORTS (S-49 to S-56)
| Screen | Route | Key Elements |
|---|---|---|
| S-49 Inventory Movement | /reports/inventory-movement | Open/Close stock per period; group by SKU |
| S-50 Current Stock Report | /reports/current-stock | Snapshot report with filters |
| S-51 Expiry Report | /reports/expiry | Color-coded days remaining |
| S-52 Transaction History | /reports/transactions | Full audit of all GRN/GDN/Transfer |
| S-53 Performance Report | /reports/performance | Processing time KPIs |
| S-54 Supplier Report | /reports/by-supplier | Supplier-level purchase summary |
| S-55 Customer Report | /reports/by-customer | Customer-level dispatch summary |
| S-56 Count Discrepancy | /reports/count-discrepancy | Historical variance trends |

All report screens share: Filter panel (collapsible) + Table + Export Excel/PDF buttons

### SYSTEM ADMIN (S-57 to S-60)
| Screen | Route | Key Elements |
|---|---|---|
| S-57 User Management | /admin/users | Table + modal for create/edit + role assignment |
| S-58 Roles & Permissions | /admin/roles | Permission matrix view (read-only display) |
| S-59 System Settings | /admin/settings | SMTP config, alert thresholds, defaults |
| S-60 Notifications | /notifications | Notification list, mark read, filter by type |

---

## Detailed Screen Specs (Key Screens)

### S-04: Main Dashboard
**Layout:** 2-column grid

| Widget | Size | Role hiển thị |
|---|---|---|
| KPI Cards (4 cards ngang) | Full width | All |
| - Tổng SKU đang hoạt động | 1/4 | All |
| - Giao dịch hôm nay (GRN + GDN) | 1/4 | All |
| - Phiếu chờ duyệt | 1/4 | Manager, Director |
| - Cảnh báo tồn thấp | 1/4 | Manager, INV_CTRL |
| Biểu đồ Nhập-Xuất 7 ngày | 2/3 | Manager, Director |
| Phiếu chờ duyệt (danh sách nhanh) | 1/3 | Manager, Director |
| Top 10 SKU tồn thấp | 1/2 | Manager, INV_CTRL |
| Batch sắp hết hạn (<=30 ngày) | 1/2 | Manager, INV_CTRL |
| Hoạt động gần nhất (Timeline) | Full width | Staff+ |

### S-25: Create GRN (Tạo Phiếu Nhập)
**Layout:** 2-panel (form header + dynamic line table)

| Section | Chi tiết |
|---|---|
| Header Form | NCC (autocomplete), Kho, Liên kết PO (optional), Ngày nhận, Ghi chú |
| Lines Table | Button "Thêm dòng" -> Row: Sản phẩm, Số lô, Ngày SX, Hạn dùng, Số lượng, Bin (dropdown path), Đơn giá |
| Bin Selector | Dropdown searchable hiển thị path đầy đủ: WH1-A-R1-S2-B3 |
| Đính kèm | Drag & drop upload khu vực |
| Actions | Lưu nháp, Gửi duyệt |

### S-31: Create GDN — Với FEFO Suggestion
| Section | Chi tiết |
|---|---|
| Header Form | KH (autocomplete), Kho, Liên kết Yêu cầu xuất, Địa chỉ giao, Ngày, Ghi chú |
| Lines Table | Thêm dòng: Sản phẩm -> hệ thống auto-load FEFO suggestion |
| FEFO Panel | Hiển thị Batch suggestions theo thứ tự FEFO: [Lô số] [Hạn: DD/MM/YYYY] [Vị trí] [Khả dụng: X] |
| Allocation | Staff xác nhận số lượng lấy từ mỗi Batch (có thể split nhiều Batch) |
| Visual Alert | Cảnh báo màu vàng nếu Batch sắp hết hạn, đỏ nếu đã hết hạn |

### S-33: Picking Screen
**Design:** Tập trung vào thao tác kho — font lớn, thông tin tối giản

| Element | Chi tiết |
|---|---|
| GDN Info | Số phiếu, KH, ngày |
| Picking List | Từng dòng: Sản phẩm, Lô số, Vị trí (lớn, dễ đọc), Số lượng cần lấy, Checkbox "Đã lấy" |
| Progress Bar | X/Y dòng đã hoàn thành |
| Button | "Hoàn thành Picking" (chỉ enable khi tất cả dòng checked) |

### S-47: Approval Inbox
**Design:** Ưu tiên UX tốc độ duyệt

| Element | Chi tiết |
|---|---|
| Filter Tabs | Chờ tôi duyệt / Đã duyệt / Đã từ chối |
| Table | Loại phiếu, Số phiếu, Người tạo, Ngày tạo, Tổng giá trị (nếu có), Cấp duyệt |
| Badge | "L1 - Trưởng kho" hoặc "L2 - Giám đốc" |
| Row Action | Preview nhanh (tooltip/hover), Xem chi tiết, Duyệt / Từ chối |
| Bulk Action | Chọn nhiều -> Duyệt tất cả (với comment chung) |

---

## Color Coding Standards

| Status | Color | Hex |
|---|---|---|
| DRAFT | Gray | #6B7280 |
| PENDING (L1/L2) | Amber | #F59E0B |
| APPROVED | Green | #22C55E |
| REJECTED | Red | #EF4444 |
| CANCELLED | Light Gray | #9CA3AF |
| DELIVERED/COMPLETED | Blue | #3B82F6 |
| EXPIRED batch | Dark Red | #DC2626 |
| EXPIRING SOON | Orange | #F97316 |
| BELOW MIN STOCK | Red row bg | #FEE2E2 |

---

## Responsive Behavior

| Viewport | Sidebar | Table |
|---|---|---|
| >= 1200px Desktop | Full 250px | All columns |
| 992-1199px Laptop | 200px | Hide minor columns |
| 768-991px Tablet | Icon only (collapsed) | Horizontal scroll |
| < 768px Mobile | Hidden (hamburger) | Card view |
