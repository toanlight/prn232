# WMS — System Architecture
# Version 1.0

---

## 1. Architecture Overview

```
+-----------------------------------------------------------------------+
|                          PRESENTATION LAYER                           |
|                                                                        |
|   +------------------------------------------------------------+     |
|   |           ASP.NET Web (MVC / Razor Pages)                  |     |
|   |                                                              |     |
|   |  - Bootstrap 5 + Custom CSS (Enterprise Dashboard UI)       |     |
|   |  - i18next - VI / EN language switching (client-side)       |     |
|   |  - Chart.js - Dashboard charts                              |     |
|   |  - DataTables - Paginated grids                             |     |
|   |  - jQuery / Axios - API calls                                |     |
|   |  - SignalR Client - Real-time notifications                  |     |
|   +------------------------------------------------------------+     |
+-----------------------------------------------------------------------+
                              |  HTTPS
                              v
+-----------------------------------------------------------------------+
|                             API LAYER                                 |
|                                                                        |
|   +------------------------------------------------------------+     |
|   |              ASP.NET Web API (.NET 8)                       |     |
|   |                                                              |     |
|   |  +----------+  +----------+  +----------+  +------------+  |     |
|   |  |Controllers|  |   JWT    |  |  RBAC    |  |  Swagger   |  |     |
|   |  | (REST)    |  |  Auth    |  |Middleware|  |  /OpenAPI  |  |     |
|   |  +----------+  +----------+  +----------+  +------------+  |     |
|   |                                                              |     |
|   |  +----------+  +----------+  +----------+  +------------+  |     |
|   |  |FluentVal |  |AutoMapper|  | SignalR  |  |  Rate Limit|  |     |
|   |  |(Validation)| |(DTO Map) |  |   Hub    |  |  (100/min) |  |     |
|   |  +----------+  +----------+  +----------+  +------------+  |     |
|   +------------------------------------------------------------+     |
+-----------------------------------------------------------------------+
                              |
                              v
+-----------------------------------------------------------------------+
|                          BUSINESS LAYER                                |
|                                                                        |
|   +--------------+  +--------------+  +----------------------------+ |
|   |  GRN Service |  |  GDN Service |  |     Approval Engine         | |
|   |  (FEFO logic)|  |  (Picking)   |  |  (State Machine 2-level)    | |
|   +--------------+  +--------------+  +----------------------------+ |
|                                                                        |
|   +--------------+  +--------------+  +----------------------------+ |
|   | Stock Service|  | Count Service|  |   Notification Service      | |
|   | (BinStock    |  | (Variance    |  |   (Email + In-app)          | |
|   |  management) |  |  detection)  |  |                             | |
|   +--------------+  +--------------+  +----------------------------+ |
|                                                                        |
|   +--------------+  +--------------+  +----------------------------+ |
|   |Report Service|  | Email Service|  |  Background Job Service     | |
|   |(EPPlus Excel |  | (MailKit     |  |  (IHostedService / Timer)   | |
|   | QuestPDF PDF)|  |  SMTP)       |  |                             | |
|   +--------------+  +--------------+  +----------------------------+ |
+-----------------------------------------------------------------------+
                              |
                              v
+-----------------------------------------------------------------------+
|                          DATA LAYER                                    |
|                                                                        |
|   +---------------------------------------+                          |
|   |        Entity Framework Core 8        |                          |
|   |  (Code First / Repository Pattern)    |                          |
|   +---------------------------------------+                          |
|                                                                        |
|   +---------------------------------------+                          |
|   |         SQL Server 2019+              |                          |
|   |                                       |                          |
|   |  - WMS_DB (main database)             |                          |
|   |  - Collation: Vietnamese_CI_AS        |                          |
|   |  - Auto daily backup                  |                          |
|   +---------------------------------------+                          |
+-----------------------------------------------------------------------+
                              |
                              v
+-----------------------------------------------------------------------+
|                     INFRASTRUCTURE / EXTERNAL                          |
|                                                                        |
|   +----------------+   +----------------+   +----------------------+ |
|   |  SMTP Server   |   |  File Storage  |   |   Background Jobs    | |
|   |  (Email Alert) |   |  (Local / NAS) |   |  (IHostedService)     | |
|   +----------------+   +----------------+   +----------------------+ |
+-----------------------------------------------------------------------+
```

---

## 2. Technology Stack

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Frontend | ASP.NET Web (Razor Pages/MVC) | .NET 8 | UI framework |
| UI Library | Bootstrap 5 | 5.3 | Responsive Enterprise design |
| Charts | Chart.js | 4.x | Dashboard charts |
| Data Grid | DataTables | 1.13 | Paginated sortable tables |
| HTTP Client | Axios / jQuery AJAX | Latest | API calls from frontend |
| Real-time | SignalR | .NET 8 | Live notifications badge |
| i18n | Client-side JSON files | Custom | VI/EN switching |
| Backend | ASP.NET Web API | .NET 8 | REST API |
| ORM | Entity Framework Core | 8.x | DB access, migrations |
| Validation | FluentValidation | 11.x | Request DTO validation |
| Mapping | AutoMapper | 12.x | Entity ↔ DTO mapping |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer | 8.x | JWT tokens |
| Email | MailKit | 4.x | SMTP email sending |
| Excel Export | EPPlus | 7.x | .xlsx export |
| PDF Export | QuestPDF | 2024.x | .pdf report generation |
| Scheduling | IHostedService + Timer | .NET 8 | Background jobs (simple setup) |
| Database | SQL Server | 2019+ | Primary data store |
| API Docs | Swashbuckle (Swagger) | Latest | OpenAPI documentation |
| Logging | Serilog | Latest | Structured logging to file |

---

## 3. Project Solution Structure

```
WMS.sln
├── WMS.Web/                      <- ASP.NET Web (Frontend)
│   ├── Controllers/
│   ├── Views/
│   │   ├── Auth/
│   │   ├── Dashboard/
│   │   ├── Products/
│   │   ├── GRN/
│   │   ├── GDN/
│   │   ├── Transfers/
│   │   ├── StockCounts/
│   │   └── Reports/
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── i18n/
│   │       ├── vi.json
│   │       └── en.json
│   └── appsettings.json
│
├── WMS.API/                      <- ASP.NET Web API (Backend)
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── WarehousesController.cs
│   │   ├── ProductsController.cs
│   │   ├── SuppliersController.cs
│   │   ├── CustomersController.cs
│   │   ├── BatchesController.cs
│   │   ├── StockController.cs
│   │   ├── GRNController.cs
│   │   ├── GDNController.cs
│   │   ├── TransfersController.cs
│   │   ├── ApprovalsController.cs
│   │   ├── StockCountsController.cs
│   │   ├── StockAdjustmentsController.cs
│   │   ├── ReportsController.cs
│   │   ├── DashboardController.cs
│   │   ├── NotificationsController.cs
│   │   └── SettingsController.cs
│   ├── Hubs/
│   │   └── NotificationHub.cs     <- SignalR Hub
│   ├── Middleware/
│   │   ├── JwtMiddleware.cs
│   │   └── ExceptionHandlerMiddleware.cs
│   └── Program.cs
│
├── WMS.Application/              <- Business Logic Layer
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── GRNService.cs
│   │   ├── GDNService.cs
│   │   ├── StockService.cs         <- FEFO logic here
│   │   ├── ApprovalService.cs      <- State machine
│   │   ├── TransferService.cs
│   │   ├── StockCountService.cs
│   │   ├── ReportService.cs
│   │   ├── EmailService.cs
│   │   └── NotificationService.cs
│   ├── BackgroundJobs/
│   │   ├── ExpiryAlertJob.cs
│   │   ├── LowStockAlertJob.cs
│   │   └── PendingApprovalReminderJob.cs
│   ├── DTOs/                      <- Request/Response DTOs
│   ├── Validators/                <- FluentValidation validators
│   ├── Mappings/                  <- AutoMapper profiles
│   └── Interfaces/                <- Service interfaces
│
├── WMS.Domain/                   <- Domain Entities
│   ├── Entities/                  (theo file 02_ERD_CodeFirst_Models.md)
│   ├── Enums/
│   │   ├── DocumentStatus.cs
│   │   ├── ApprovalStatus.cs
│   │   ├── CustomerType.cs
│   │   └── StockTxnType.cs
│   └── Constants/
│       └── RoleCodes.cs
│
├── WMS.Infrastructure/           <- Data Access Layer
│   ├── Data/
│   │   ├── WmsDbContext.cs
│   │   ├── Configurations/        <- EF Fluent API configs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── GenericRepository.cs
│   │   ├── GRNRepository.cs
│   │   ├── StockRepository.cs     <- Complex FEFO queries
│   │   └── ReportRepository.cs    <- Raw SQL for reports
│   └── Email/
│       ├── EmailSender.cs
│       └── Templates/
│           ├── ApprovalNotification.html
│           ├── LowStockAlert.html
│           └── ExpiryAlert.html
│
└── WMS.Tests/                    <- Unit Tests
    ├── Services/
    │   ├── StockServiceTests.cs   <- FEFO logic tests
    │   ├── GRNServiceTests.cs
    │   └── ApprovalServiceTests.cs
    └── Fixtures/
```

---

## 4. Approval State Machine

```
Document Created
      |
      v
   [DRAFT]
      | Submit()
      v
[PENDING_L1] ---- WH_MANAGER Reject ----> [REJECTED]
      |                                        |
      | WH_MANAGER Approve                     v
      v                                [Nguoi tao huy]
[PENDING_L2] ---- DIRECTOR Reject -----> [REJECTED]
      |
      | DIRECTOR Approve
      v
  [APPROVED]
      |
      | (GDN only: Picking flow)
      v
  [PICKING] -> [PICKED] -> [DELIVERED]
      |
      | (TRANSFER: Execute transfer)
      v
  [COMPLETED]
      |
      | (User cancels DRAFT/REJECTED)
      v
 [CANCELLED]
```

---

## 5. FEFO Algorithm

```csharp
// StockService.GetFEFOSuggestion(productId, requestedQty, warehouseId)
public async Task<List<FEFOSuggestionDto>> GetFEFOSuggestion(
    int productId, decimal requestedQty, int warehouseId)
{
    // Query vw_FEFOStock: ORDER BY ExpiryDate ASC, BatchId ASC
    // Filter: AvailableQty > 0, Status = 'ACTIVE', WarehouseId matches
    // Allocate greedily: fill from earliest expiry first
    // Return list of {BatchId, BinId, AllocatedQty}
}
```

---

## 6. Security Architecture

```
Client --HTTPS--> ASP.NET API
                        |
                        +-- [Rate Limiter Middleware] 100 req/min/IP
                        |
                        +-- [JWT Validation Middleware]
                        |     Validates Bearer token signature + expiry
                        |
                        +-- [RBAC Authorization Middleware]
                        |     [Authorize(Roles = "WH_MANAGER,DIRECTOR")]
                        |
                        +-- [Controller Action]
```

**Token Flow:**
1. Login → AccessToken (1h) + RefreshToken (7 days, stored in DB)
2. API calls → `Authorization: Bearer {accessToken}`
3. Access token expired → `POST /auth/refresh` with refresh token
4. Logout → Refresh token revoked in DB

---

## 7. Background Jobs Schedule

| Job | Trigger | Description |
|---|---|---|
| ExpiryAlertJob | Daily 07:00 | Scan Batches expiring within N days |
| LowStockAlertJob | Daily 07:30 | Scan Products below MinStock |
| PendingApprovalReminderJob | Daily 08:00 | Remind for pending > 24h |
| BatchStatusUpdateJob | Daily 00:00 | Auto-mark Batches as EXPIRED |

---

## 8. Deployment Configuration

```
Server: Windows Server 2019+ / IIS 10+

Recommended minimum specs (small/medium warehouse):
  CPU: 4 cores
  RAM: 8 GB
  Storage: 100 GB SSD (OS + App) + 500 GB (DB + Backups)

Directory Layout:
  C:\inetpub\wms-web\          <- ASP.NET Web published output
  C:\inetpub\wms-api\          <- ASP.NET API published output
  D:\WMS\Uploads\              <- File attachments
  D:\WMS\Backups\              <- DB backup files
  D:\WMS\Logs\                 <- Serilog log files

IIS Sites:
  wms.company.local            -> WMS.Web (port 80, redirect to 443)
  api.wms.company.local        -> WMS.API (port 443)

SSL: Self-signed cert for internal network / Let's Encrypt for internet exposure

SQL Server Backup:
  Full backup: Daily 02:00 -> D:\WMS\Backups\
  Retention: 30 days rolling
```

---

## 9. ERD Summary (Entity Relationship)

```
Users -------------- UserRoles -------- Roles
  |
  +-- Warehouses
  |       +-- Zones
  |               +-- Racks
  |                       +-- Shelves
  |                               +-- Bins ---------- BinStocks --+
  |                                                        |       |
Suppliers ----------- Batches --------------------------- +       |
                          |                                        |
Products ------------ Batches                                      |
    |                                                               |
    |   GoodsReceiptNotes ---- GRNLines --- Batches --- BinStocks |
    |       |                                                       |
Suppliers --+                                                       |
                                                                    |
    |   GoodsDispatchNotes -- GDNLines ---- Batches --- BinStocks |
    |       |                                                       |
Customers --+                              +---------------------- +

TransferOrders ---------- TransferOrderLines

StockCounts -------------- StockCountLines
    |
    +-- StockAdjustments -- StockAdjustmentLines

Approvals <--(DocumentType + DocumentId)--> GRN / GDN / Transfer / Adj

StockTransactions <-- All stock movements (immutable ledger)
```

---

## 10. Key Architectural Decisions

| Decision | Rationale |
|---|---|
| Layered architecture (Web / API / Application / Domain / Infrastructure) | Tách biệt UI, business logic, data access — dễ test và maintain với team 2 người |
| BinStock là bảng trung tâm cho tồn kho realtime | Tránh phải SUM() từ StockTransaction mỗi lần query, đảm bảo performance |
| StockTransaction là ledger bất biến (append-only) | Cung cấp audit trail đầy đủ cho báo cáo Nhập-Xuất-Tồn mà không cần Audit Log riêng |
| Approval polymorphic (DocumentType + DocumentId) | Dùng chung 1 engine duyệt 2 cấp cho GRN/GDN/Transfer/Adjustment, tránh trùng lặp code |
| IHostedService thay vì Hangfire | Quy mô nhỏ (<200 giao dịch/ngày) không cần dependency ngoài, giảm độ phức tạp vận hành |
| SignalR cho notification realtime | UX tốt hơn polling, chi phí thấp với <50 concurrent users |
| Single Tenant, không multi-tenant | Đúng theo yêu cầu nghiệp vụ đã chốt, đơn giản hóa schema (không cần TenantId ở mọi bảng) |
