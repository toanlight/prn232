# WMS — System Architecture (N-Layer Architecture)
# Version 1.1

---

## 1. Architecture Overview

```
+-----------------------------------------------------------------------+
|                          WMS.Web (MVC Client)                         |
|                                                                        |
|  - Controllers calling REST API via HttpClient                        |
|  - Bootstrap 5 + Custom CSS (Enterprise Dashboard UI)                 |
|  - i18next - VI / EN language switching (client-side)                 |
|  - Chart.js - Dashboard charts                                        |
|  - DataTables - Paginated grids                                       |
+-----------------------------------------------------------------------+
                               |  HTTP / HTTPS (HttpClient)
                               v
+-----------------------------------------------------------------------+
|                          WMS.API (RESTful API)                        |
|                                                                        |
|  - API Controllers (Auth, Products, Stock, GRN, GDN, Approvals...)    |
|  - JWT Authentication & RBAC Authorization Middleware                 |
|  - Swagger / OpenAPI Documentation                                    |
|  - SignalR Notification Hub                                           |
+-----------------------------------------------------------------------+
                               |  Direct Reference
                               v
+-----------------------------------------------------------------------+
|                        Services (Business Logic)                      |
|                                                                        |
|  - GRNService, GDNService (FEFO Algorithm)                            |
|  - StockService (BinStock Management & Ledger)                        |
|  - ApprovalService (2-Level Approval Engine)                          |
|  - ReportService, NotificationService, EmailService                   |
|  - DTO Mapping & FluentValidation                                     |
+-----------------------------------------------------------------------+
                               |  Direct Reference
                               v
+-----------------------------------------------------------------------+
|                      Repositories (Data Repository)                   |
|                                                                        |
|  - IGenericRepository<T> & GenericRepository<T>                       |
|  - IProductRepository, IBinStockRepository, IGRNRepository...         |
|  - Calls DAL / DbContext to query database                            |
+-----------------------------------------------------------------------+
                               |  Direct Reference
                               v
+-----------------------------------------------------------------------+
|                   DataAccessLayer (DAL / DAO / EF Core)               |
|                                                                        |
|  - WmsDbContext (DbSet<T> for all 26 entities)                        |
|  - Fluent API Configurations                                          |
|  - EF Core Migrations (Code-First Migration Tooling)                  |
|  - Data Access Objects (DAOs)                                         |
+-----------------------------------------------------------------------+
                               |  Direct Reference
                               v
+-----------------------------------------------------------------------+
|                   BusinessLayer (Business Objects / Domain)           |
|                                                                        |
|  - 26 POCO Entity Models (User, Product, BinStock, GRN, GDN...)      |
|  - Enums (DocumentStatus, ApprovalLevel, CustomerType...)             |
|  - Base Classes (BaseEntity, IAuditable)                              |
|  - Request / Response DTOs                                            |
+-----------------------------------------------------------------------+
                               |  SQL Client Connection
                               v
+-----------------------------------------------------------------------+
|                       SQL Server 2019+ Database                       |
|  - Primary Data Store (WMS_DB)                                        |
|  - Collation: Vietnamese_CI_AS                                        |
+-----------------------------------------------------------------------+
```

---

## 2. Technology Stack

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Frontend Client | WMS.Web (ASP.NET Core MVC) | .NET 8 | UI framework with Razor Views |
| UI Library | Bootstrap 5 | 5.3 | Responsive Enterprise design |
| Charts | Chart.js | 4.x | Dashboard charts |
| Data Grid | DataTables | 1.13 | Paginated sortable tables |
| HTTP Client | HttpClient / IHttpClientFactory | .NET 8 | Calling WMS.API REST endpoints |
| Real-time | SignalR | .NET 8 | Live notifications badge |
| i18n | Client-side JSON files | Custom | VI/EN switching |
| API Layer | WMS.API (ASP.NET Web API) | .NET 8 | REST API Endpoints |
| Business Logic | Services Layer | .NET 8 | FEFO, Approvals, DTO Mapping, Business validation |
| Data Access | Repositories Layer | .NET 8 | Interface & Generic/Custom Repositories |
| DAL & ORM | DataAccessLayer (EF Core 8) | 8.x | WmsDbContext, EF Migrations, Fluent API |
| Models & Domain | BusinessLayer | .NET 8 | POCO Entities, Enums, DTOs |
| Validation | FluentValidation | 11.x | Request DTO validation |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer | 8.x | JWT tokens |
| Email | MailKit | 4.x | SMTP email sending |
| Excel Export | EPPlus | 7.x | .xlsx export |
| PDF Export | QuestPDF | 2024.x | .pdf report generation |
| Database | SQL Server | 2019+ | Primary data store |
| API Docs | Swashbuckle (Swagger) | Latest | OpenAPI documentation |

---

## 3. Project Solution Structure (N-Layer)

```
Project_Final.sln
├── WMS.Web/                      <- Presentation Layer (MVC Client)
│   ├── Controllers/               <- Call WMS.API via HttpClient
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
│   └── appsettings.json
│
├── WMS.API/                      <- API Layer (RESTful Endpoints)
│   ├── Controllers/               <- Call Services layer
│   │   ├── AuthController.cs
│   │   ├── ProductsController.cs
│   │   ├── StockController.cs
│   │   ├── GRNController.cs
│   │   ├── GDNController.cs
│   │   ├── ApprovalsController.cs
│   │   └── ...
│   ├── Hubs/                      <- SignalR Hub
│   ├── Middleware/                <- JWT & Exception Middlewares
│   └── Program.cs                 <- Dependency Injection Registration
│
├── Services/                     <- Business Logic Layer (BLL)
│   ├── Interfaces/                <- Service interfaces (IProductService, IStockService...)
│   ├── Implementations/
│   │   ├── AuthService.cs
│   │   ├── GRNService.cs
│   │   ├── GDNService.cs          <- FEFO logic implementation
│   │   ├── StockService.cs        <- BinStock & StockTransaction logic
│   │   ├── ApprovalService.cs     <- State machine approval engine
│   │   ├── TransferService.cs
│   │   ├── StockCountService.cs
│   │   └── ReportService.cs
│   └── Mappings/                  <- Entity <-> DTO Mapping profiles
│
├── Repositories/                 <- Repository Layer
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── IBinStockRepository.cs
│   │   └── IGRNRepository.cs
│   └── Implementations/
│       ├── GenericRepository.cs
│       ├── ProductRepository.cs
│       ├── BinStockRepository.cs
│       └── GRNRepository.cs
│
├── DataAccessLayer/              <- Data Access Layer (DAL / DAO / EF Core)
│   ├── WmsDbContext.cs            <- EF Core DbContext
│   ├── Configurations/            <- Fluent API Entity Configurations
│   ├── Migrations/                <- EF Core Code-First Migrations
│   └── DAO/                       <- Data Access Objects (Direct DB helpers)
│
└── BusinessLayer/                <- Business Objects / Domain Layer
    ├── Entities/                  <- 26 POCO Entity Classes
    ├── Enums/                     <- DocumentStatus, ApprovalLevel, CustomerType...
    ├── Common/                    <- BaseEntity, IAuditable
    └── DTOs/                      <- Request / Response Data Transfer Objects
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

## 5. FEFO Algorithm (in Services Layer)

```csharp
// StockService.GetFEFOSuggestion(productId, requestedQty, warehouseId)
public async Task<List<FEFOSuggestionDto>> GetFEFOSuggestion(
    int productId, decimal requestedQty, int warehouseId)
{
    // Query BinStocks via IBinStockRepository: ORDER BY ExpiryDate ASC, BatchId ASC
    // Filter: AvailableQty > 0, Status = 'ACTIVE', WarehouseId matches
    // Allocate greedily: fill from earliest expiry first
    // Return list of {BatchId, BinId, AllocatedQty}
}
```

---

## 6. Security Architecture

```
Client (WMS.Web) --HTTPS--> WMS.API
                             |
                             +-- [JWT Validation Middleware]
                             |     Validates Bearer token signature + expiry
                             |
                             +-- [RBAC Authorization Middleware]
                             |     [Authorize(Roles = "WH_MANAGER,DIRECTOR")]
                             |
                             +-- [Controller Action] -> Services -> Repositories -> DAL
```

**Token Flow:**
1. Login → AccessToken (1h) + RefreshToken (7 days, stored in DB)
2. API calls → `Authorization: Bearer {accessToken}`
3. Access token expired → `POST /auth/refresh` with refresh token
4. Logout → Refresh token revoked in DB

---

## 7. Key Architectural Decisions

| Decision | Rationale |
|---|---|
| N-Layer architecture (`WMS.Web` -> `WMS.API` -> `Services` -> `Repositories` -> `DataAccessLayer` -> `BusinessLayer`) | Đun chuẩn mô hình kiến trúc môn học PRN232, phân tách rõ ràng trách nhiệm từng tầng |
| Entities, Enums & DTOs ở `BusinessLayer` | `BusinessLayer` đóng vai trò Domain/BusinessObject, cung cấp POCO Entities và DTOs cho toàn hệ thống |
| DbContext & Migrations ở `DataAccessLayer` | `DataAccessLayer` quản lý `WmsDbContext`, Fluent API Configurations và EF Migrations |
| Repositories Layer riêng biệt | Tránh phụ thuộc trực tiếp giữa Services và `DbContext`, tuân thủ Dependency Inversion |
| BinStock là bảng trung tâm cho tồn kho realtime | Tránh phải SUM() từ StockTransaction mỗi lần query, đảm bảo performance |
| StockTransaction là ledger bất biến (append-only) | Cung cấp audit trail đầy đủ cho báo cáo Nhập-Xuất-Tồn |
| Approval polymorphic (DocumentType + DocumentId) | Dùng chung 1 engine duyệt 2 cấp cho GRN/GDN/Transfer/Adjustment |
