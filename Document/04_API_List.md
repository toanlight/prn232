# WMS — RESTful API Specification
# Version 1.0 | Base URL: /api/v1

Tất cả request phải kèm header:
```
Authorization: Bearer {access_token}
Accept-Language: vi | en
```

Tất cả response theo format:
```json
{
  "success": true,
  "data": { },
  "message": "string",
  "errors": [],
  "meta": { "page": 1, "pageSize": 20, "total": 150 }
}
```

---

## 1. AUTH — /api/v1/auth

| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | /auth/login | Đăng nhập, trả về access_token + refresh_token | Public |
| POST | /auth/refresh | Làm mới access_token bằng refresh_token | Public |
| POST | /auth/logout | Revoke refresh_token | Authenticated |
| POST | /auth/change-password | Đổi mật khẩu | Authenticated |
| POST | /auth/forgot-password | Gửi email reset password | Public |
| POST | /auth/reset-password | Xác nhận reset password bằng token email | Public |

**POST /auth/login Request:**
```json
{ "username": "string", "password": "string" }
```
**Response:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "abc...",
  "expiresIn": 3600,
  "user": { "userId": 1, "fullName": "...", "roles": ["WH_MANAGER"], "preferredLang": "vi" }
}
```

---

## 2. USERS — /api/v1/users

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /users | Danh sách users (phân trang, tìm kiếm) | SYS_ADMIN |
| POST | /users | Tạo user mới | SYS_ADMIN |
| GET | /users/{id} | Chi tiết user | SYS_ADMIN |
| PUT | /users/{id} | Cập nhật thông tin user | SYS_ADMIN |
| PATCH | /users/{id}/activate | Kích hoạt / vô hiệu hóa user | SYS_ADMIN |
| PUT | /users/{id}/roles | Gán lại danh sách role cho user | SYS_ADMIN |
| GET | /users/me | Thông tin user hiện tại | Authenticated |
| PUT | /users/me/profile | Cập nhật profile cá nhân | Authenticated |
| PATCH | /users/me/language | Đổi ngôn ngữ ưu tiên | Authenticated |

---

## 3. WAREHOUSE STRUCTURE

### Warehouses — /api/v1/warehouses

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /warehouses | Danh sách kho | All |
| POST | /warehouses | Tạo kho mới | SYS_ADMIN, WH_MANAGER |
| GET | /warehouses/{id} | Chi tiết kho | All |
| PUT | /warehouses/{id} | Cập nhật kho | SYS_ADMIN, WH_MANAGER |
| GET | /warehouses/{id}/tree | Cấu trúc cây đầy đủ (Zones→Racks→Shelves→Bins) | All |
| GET | /warehouses/{id}/stock-summary | Tổng tồn kho theo kho | WH_MANAGER+ |

### Zones — /api/v1/warehouses/{warehouseId}/zones

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /zones | Danh sách Zone theo kho | All |
| POST | /zones | Tạo Zone | WH_MANAGER+ |
| GET | /zones/{id} | Chi tiết Zone | All |
| PUT | /zones/{id} | Cập nhật Zone | WH_MANAGER+ |
| DELETE | /zones/{id} | Xóa Zone (chỉ khi không có hàng tồn) | SYS_ADMIN |

### Racks — /api/v1/zones/{zoneId}/racks

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /racks | Danh sách Rack theo Zone | All |
| POST | /racks | Tạo Rack | WH_MANAGER+ |
| PUT | /racks/{id} | Cập nhật Rack | WH_MANAGER+ |

### Shelves — /api/v1/racks/{rackId}/shelves

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /shelves | Danh sách Shelf theo Rack | All |
| POST | /shelves | Tạo Shelf | WH_MANAGER+ |
| PUT | /shelves/{id} | Cập nhật Shelf | WH_MANAGER+ |

### Bins — /api/v1/shelves/{shelfId}/bins

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /bins | Danh sách Bin theo Shelf | All |
| POST | /bins | Tạo Bin | WH_MANAGER+ |
| PUT | /bins/{id} | Cập nhật Bin | WH_MANAGER+ |
| GET | /bins/{id}/stock | Tồn kho chi tiết tại Bin | WH_STAFF+ |

---

## 4. PRODUCTS — /api/v1/products

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /products | Danh sách sản phẩm (filter: category, isActive, search) | All |
| POST | /products | Tạo sản phẩm | SYS_ADMIN, WH_MANAGER, PURCHASING |
| GET | /products/{id} | Chi tiết sản phẩm | All |
| PUT | /products/{id} | Cập nhật sản phẩm | SYS_ADMIN, WH_MANAGER, PURCHASING |
| PATCH | /products/{id}/activate | Kích hoạt / vô hiệu hóa | WH_MANAGER+ |
| GET | /products/{id}/stock | Tồn kho theo sản phẩm (all Bins + Batches) | All |
| GET | /products/{id}/transactions | Lịch sử giao dịch sản phẩm | WH_MANAGER+ |
| GET | /products/low-stock | DS sản phẩm dưới ngưỡng Min Stock | WH_MANAGER+ |

### Product Categories — /api/v1/categories

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /categories | Danh sách danh mục (tree) | All |
| POST | /categories | Tạo danh mục | SYS_ADMIN, WH_MANAGER |
| PUT | /categories/{id} | Cập nhật danh mục | SYS_ADMIN, WH_MANAGER |
| DELETE | /categories/{id} | Xóa (nếu không có product) | SYS_ADMIN |

---

## 5. SUPPLIERS — /api/v1/suppliers

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /suppliers | Danh sách NCC (filter: status, search) | WH_MANAGER, PURCHASING, SALES, DIRECTOR |
| POST | /suppliers | Tạo NCC | PURCHASING, SYS_ADMIN |
| GET | /suppliers/{id} | Chi tiết NCC | WH_MANAGER+, PURCHASING |
| PUT | /suppliers/{id} | Cập nhật NCC | PURCHASING, SYS_ADMIN |
| PATCH | /suppliers/{id}/status | Thay đổi trạng thái | SYS_ADMIN, PURCHASING |
| GET | /suppliers/{id}/transactions | Lịch sử nhập hàng theo NCC | PURCHASING, WH_MANAGER+ |

---

## 6. CUSTOMERS — /api/v1/customers

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /customers | Danh sách KH (filter: type, isActive) | SALES, WH_MANAGER+, DIRECTOR |
| POST | /customers | Tạo KH | SALES, SYS_ADMIN |
| GET | /customers/{id} | Chi tiết KH | SALES+, WH_MANAGER |
| PUT | /customers/{id} | Cập nhật KH | SALES, SYS_ADMIN |
| GET | /customers/{id}/transactions | Lịch sử xuất hàng theo KH | SALES, WH_MANAGER+ |

---

## 7. BATCHES — /api/v1/batches

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /batches | Danh sách lô hàng (filter: product, supplier, status, expiry) | WH_STAFF+, INV_CTRL |
| GET | /batches/{id} | Chi tiết lô hàng + tồn kho theo Bin | WH_STAFF+ |
| GET | /batches/{id}/transactions | Lịch sử chuyển động của lô | WH_MANAGER+, INV_CTRL |
| GET | /batches/expiring | DS lô sắp hết hạn (trong N ngày) | WH_MANAGER+, INV_CTRL |
| GET | /batches/expired | DS lô đã hết hạn | WH_MANAGER+, INV_CTRL |
| PATCH | /batches/{id}/status | Cập nhật trạng thái lô (RECALLED...) | WH_MANAGER, SYS_ADMIN |

---

## 8. STOCK — /api/v1/stock

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /stock | Tồn kho tổng hợp (group by Product, optional: Bin, Batch) | All |
| GET | /stock/by-location | Tồn kho theo vị trí (filter: warehouseId, zoneId, binId) | WH_STAFF+ |
| GET | /stock/by-batch | Tồn kho theo Batch (với thông tin Expiry) | WH_STAFF+, INV_CTRL |
| GET | /stock/fefo-suggestion | Đề xuất Batch theo FEFO cho một ProductId + Qty | WH_STAFF+ |
| GET | /stock/available | Tồn khả dụng (Qty - ReservedQty) theo ProductId | WH_STAFF+ |

**GET /stock/fefo-suggestion Query params:**
```
productId=123&quantity=50&warehouseId=1
```
**Response:**
```json
{
  "suggestions": [
    { "batchId": 5, "lotNumber": "LOT-001", "expiryDate": "2025-08-01", "binId": 12, "binPath": "WH1-A-R1-S2-B3", "availableQty": 30, "priority": 1 },
    { "batchId": 8, "lotNumber": "LOT-002", "expiryDate": "2025-09-15", "binId": 15, "binPath": "WH1-A-R1-S3-B1", "availableQty": 20, "priority": 2 }
  ]
}
```

---

## 9. PURCHASE ORDERS — /api/v1/purchase-orders

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /purchase-orders | Danh sách PO (filter: supplier, status, date) | PURCHASING, WH_MANAGER+ |
| POST | /purchase-orders | Tạo PO mới | PURCHASING, SYS_ADMIN |
| GET | /purchase-orders/{id} | Chi tiết PO + lines | PURCHASING, WH_STAFF+ |
| PUT | /purchase-orders/{id} | Cập nhật PO (chỉ khi DRAFT) | PURCHASING |
| PATCH | /purchase-orders/{id}/submit | Submit PO (DRAFT → SUBMITTED) | PURCHASING |
| PATCH | /purchase-orders/{id}/cancel | Hủy PO | PURCHASING, SYS_ADMIN |

---

## 10. GRN (GOODS RECEIPT NOTES) — /api/v1/grn

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /grn | Danh sách phiếu nhập (filter: status, date, supplier, warehouse) | WH_STAFF+, PURCHASING |
| POST | /grn | Tạo phiếu nhập mới (DRAFT) | WH_STAFF, WH_MANAGER, PURCHASING |
| GET | /grn/{id} | Chi tiết phiếu nhập + lines + approval timeline | WH_STAFF+ |
| PUT | /grn/{id} | Cập nhật phiếu (chỉ khi DRAFT) | WH_STAFF, WH_MANAGER |
| PATCH | /grn/{id}/submit | Submit để duyệt (DRAFT → PENDING_L1) | WH_STAFF+ |
| PATCH | /grn/{id}/cancel | Hủy phiếu (DRAFT hoặc REJECTED) | WH_STAFF+ (người tạo) |
| POST | /grn/{id}/attachments | Upload file đính kèm | WH_STAFF+ |
| DELETE | /grn/{id}/attachments/{fileId} | Xóa file đính kèm | WH_STAFF+ |
| GET | /grn/{id}/approval-history | Lịch sử phê duyệt | WH_STAFF+ |

**POST /grn Request Body:**
```json
{
  "supplierId": 1,
  "warehouseId": 1,
  "poId": 5,
  "receiptDate": "2025-06-19",
  "notes": "Hang dat chat luong",
  "lines": [
    {
      "productId": 10,
      "quantity": 100,
      "binId": 3,
      "lotNumber": "LOT-2025-001",
      "mfgDate": "2025-01-01",
      "expiryDate": "2026-01-01",
      "unitPrice": 50000
    }
  ]
}
```

---

## 11. GDN (GOODS DISPATCH NOTES) — /api/v1/gdn

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /gdn | Danh sách phiếu xuất (filter: status, date, customer) | WH_STAFF+, SALES |
| POST | /gdn | Tạo phiếu xuất mới (DRAFT) | WH_STAFF, WH_MANAGER, SALES |
| GET | /gdn/{id} | Chi tiết phiếu xuất + lines + approval timeline | WH_STAFF+, SALES |
| PUT | /gdn/{id} | Cập nhật phiếu (chỉ khi DRAFT) | WH_STAFF, WH_MANAGER, SALES |
| PATCH | /gdn/{id}/submit | Submit (DRAFT → PENDING_L1) | WH_STAFF+ |
| PATCH | /gdn/{id}/cancel | Hủy phiếu | WH_STAFF+ (người tạo), WH_MANAGER |
| PATCH | /gdn/{id}/picking | Bắt đầu picking (APPROVED → PICKING) | WH_STAFF |
| PATCH | /gdn/{id}/picked | Xác nhận đã picking xong (PICKING → PICKED) | WH_STAFF |
| PATCH | /gdn/{id}/deliver | Xác nhận giao hàng (PICKED → DELIVERED) | WH_STAFF, WH_MANAGER |
| POST | /gdn/{id}/attachments | Upload file đính kèm | WH_STAFF+ |
| GET | /dispatch-requests | Danh sách yêu cầu xuất | WH_STAFF+, SALES |
| POST | /dispatch-requests | Tạo yêu cầu xuất | SALES, WH_STAFF |

**PATCH /gdn/{id}/deliver Body:**
```json
{
  "deliveryNote": "Giao dung don, khach ky nhan",
  "actualLines": [
    { "gdnLineId": 1, "pickedQty": 50 },
    { "gdnLineId": 2, "pickedQty": 20 }
  ]
}
```

---

## 12. TRANSFER ORDERS — /api/v1/transfers

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /transfers | Danh sách phiếu chuyển kho | WH_STAFF+, INV_CTRL |
| POST | /transfers | Tạo phiếu chuyển kho | WH_STAFF, WH_MANAGER, INV_CTRL |
| GET | /transfers/{id} | Chi tiết phiếu chuyển | WH_STAFF+ |
| PUT | /transfers/{id} | Cập nhật (chỉ DRAFT) | WH_STAFF+ |
| PATCH | /transfers/{id}/submit | Submit (DRAFT → PENDING_L1) | WH_STAFF+ |
| PATCH | /transfers/{id}/cancel | Hủy phiếu | WH_STAFF+ (người tạo) |

---

## 13. APPROVALS — /api/v1/approvals

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /approvals/inbox | Danh sách phiếu cần tôi duyệt (pending) | WH_MANAGER, DIRECTOR |
| GET | /approvals/history | Lịch sử đã duyệt | WH_MANAGER, DIRECTOR |
| POST | /approvals/{id}/approve | Phê duyệt | WH_MANAGER (L1), DIRECTOR (L2) |
| POST | /approvals/{id}/reject | Từ chối | WH_MANAGER (L1), DIRECTOR (L2) |

**POST /approvals/{id}/approve Body:**
```json
{ "comment": "Phieu hop le, cho phep thuc hien" }
```

**POST /approvals/{id}/reject Body:**
```json
{ "comment": "Thieu chung tu, can bo sung truoc khi duyet" }
```

---

## 14. STOCK COUNTS — /api/v1/stock-counts

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /stock-counts | Danh sách kế hoạch kiểm kê | INV_CTRL, WH_MANAGER+, WH_STAFF |
| POST | /stock-counts | Tạo kế hoạch kiểm kê | INV_CTRL, WH_MANAGER |
| GET | /stock-counts/{id} | Chi tiết + danh sách lines | INV_CTRL, WH_STAFF+, WH_MANAGER |
| PATCH | /stock-counts/{id}/start | Bắt đầu kiểm kê (PLANNED → IN_PROGRESS) | INV_CTRL, WH_MANAGER |
| PATCH | /stock-counts/{id}/complete | Hoàn thành kiểm kê | INV_CTRL, WH_MANAGER |
| PUT | /stock-counts/{id}/lines/{lineId} | Nhập số lượng thực tế | WH_STAFF |
| GET | /stock-counts/{id}/variances | Xem chênh lệch kiểm kê | INV_CTRL, WH_MANAGER+ |

---

## 15. STOCK ADJUSTMENTS — /api/v1/stock-adjustments

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /stock-adjustments | Danh sách biên bản điều chỉnh | INV_CTRL, WH_MANAGER+ |
| POST | /stock-adjustments | Tạo biên bản (thường từ Stock Count) | INV_CTRL, WH_MANAGER |
| GET | /stock-adjustments/{id} | Chi tiết biên bản + lines | INV_CTRL, WH_MANAGER+ |
| PATCH | /stock-adjustments/{id}/submit | Submit để duyệt | INV_CTRL, WH_MANAGER |

---

## 16. REPORTS — /api/v1/reports

| Method | Endpoint | Query Params | Role |
|---|---|---|---|
| GET | /reports/current-stock | warehouseId, categoryId, belowMinStock | WH_MANAGER+, INV_CTRL |
| GET | /reports/inventory-movement | warehouseId, from, to, productId, txnType | WH_MANAGER+, INV_CTRL |
| GET | /reports/expiry | warehouseId, daysToExpiry, status | WH_MANAGER+, INV_CTRL |
| GET | /reports/transactions | from, to, docType, warehouseId, productId | WH_MANAGER+, INV_CTRL |
| GET | /reports/performance | warehouseId, from, to | WH_MANAGER, DIRECTOR |
| GET | /reports/by-supplier | supplierId, from, to | PURCHASING, WH_MANAGER+, DIRECTOR |
| GET | /reports/by-customer | customerId, from, to | SALES, WH_MANAGER+, DIRECTOR |
| GET | /reports/count-discrepancy | countId, warehouseId, from, to | INV_CTRL, WH_MANAGER+ |

### Export Endpoints

| Method | Endpoint | Format |
|---|---|---|
| GET | /reports/{reportType}/export?format=excel | .xlsx |
| GET | /reports/{reportType}/export?format=pdf | .pdf |

**Response:** Binary file stream với Content-Disposition header.

---

## 17. DASHBOARD — /api/v1/dashboard

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /dashboard/summary | KPI cards: tổng SKU, giao dịch hôm nay, cảnh báo | All |
| GET | /dashboard/movement-chart | Dữ liệu biểu đồ nhập-xuất 7/30 ngày | WH_MANAGER+ |
| GET | /dashboard/pending-approvals | Count + 5 phiếu mới nhất chờ duyệt | WH_MANAGER, DIRECTOR |
| GET | /dashboard/low-stock | Top 10 SKU tồn thấp | WH_MANAGER+, INV_CTRL |
| GET | /dashboard/expiring-batches | Batch hết hạn trong 30 ngày tới | WH_MANAGER+, INV_CTRL |
| GET | /dashboard/recent-activities | 10 giao dịch mới nhất | WH_STAFF+ |

---

## 18. NOTIFICATIONS — /api/v1/notifications

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /notifications | Danh sách thông báo (unread first) | Authenticated |
| GET | /notifications/unread-count | Số lượng chưa đọc (cho badge) | Authenticated |
| PATCH | /notifications/{id}/read | Đánh dấu đã đọc | Authenticated |
| PATCH | /notifications/read-all | Đánh dấu tất cả đã đọc | Authenticated |

---

## 19. SYSTEM SETTINGS — /api/v1/settings

| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | /settings | Lấy toàn bộ settings | SYS_ADMIN |
| PUT | /settings | Cập nhật settings (batch update) | SYS_ADMIN |
| POST | /settings/test-email | Gửi test email xác nhận SMTP | SYS_ADMIN |

---

## 20. COMMON RESPONSE CODES

| Code | Meaning |
|---|---|
| 200 | OK |
| 201 | Created |
| 204 | No Content (DELETE success) |
| 400 | Bad Request (validation failed) |
| 401 | Unauthorized (token missing/expired) |
| 403 | Forbidden (insufficient role) |
| 404 | Not Found |
| 409 | Conflict (duplicate, business rule violation) |
| 422 | Unprocessable Entity (business logic error) |
| 500 | Internal Server Error |

## 21. PAGINATION

Tất cả danh sách hỗ trợ query params:
```
?page=1&pageSize=20&sortBy=createdAt&sortDir=desc
```

## 22. COMMON FILTERS

```
?search=keyword      Full-text search tren ten/ma
?isActive=true       Loc theo trang thai
?from=2025-01-01     Ngay bat dau (ISO 8601)
?to=2025-12-31       Ngay ket thuc
?warehouseId=1       Loc theo kho
?status=PENDING_L1   Loc theo trang thai phieu
```
