-- ============================================================
-- WMS - Warehouse Import-Export Management System
-- SQL Server Database Schema
-- Version: 1.0 | Date: 2025-06-19
-- ============================================================

-- ============================================================
-- SECTION 0: DATABASE SETUP
-- ============================================================
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WMS_DB')
BEGIN
    CREATE DATABASE WMS_DB
    COLLATE Vietnamese_CI_AS;
END
GO

USE WMS_DB;
GO

-- ============================================================
-- SECTION 1: AUTHENTICATION & AUTHORIZATION
-- ============================================================

CREATE TABLE Roles (
    RoleId      INT             NOT NULL IDENTITY(1,1),
    RoleCode    VARCHAR(30)     NOT NULL,   -- SYS_ADMIN, DIRECTOR, WH_MANAGER, ...
    RoleName    NVARCHAR(100)   NOT NULL,
    RoleNameEn  NVARCHAR(100)   NOT NULL,
    Description NVARCHAR(500)   NULL,
    IsActive    BIT             NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_RoleCode UNIQUE (RoleCode)
);
GO

CREATE TABLE Users (
    UserId          INT             NOT NULL IDENTITY(1,1),
    Username        VARCHAR(50)     NOT NULL,
    Email           NVARCHAR(200)   NOT NULL,
    PasswordHash    VARCHAR(256)    NOT NULL,
    FullName        NVARCHAR(150)   NOT NULL,
    FullNameEn      NVARCHAR(150)   NULL,
    Phone           VARCHAR(20)     NULL,
    PreferredLang   CHAR(2)         NOT NULL DEFAULT 'vi',  -- vi | en
    IsActive        BIT             NOT NULL DEFAULT 1,
    LastLoginAt     DATETIME2       NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Lang CHECK (PreferredLang IN ('vi', 'en'))
);
GO

CREATE TABLE UserRoles (
    UserRoleId  INT         NOT NULL IDENTITY(1,1),
    UserId      INT         NOT NULL,
    RoleId      INT         NOT NULL,
    AssignedAt  DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    AssignedBy  INT         NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserRoleId),
    CONSTRAINT UQ_UserRoles UNIQUE (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

CREATE TABLE RefreshTokens (
    TokenId     INT             NOT NULL IDENTITY(1,1),
    UserId      INT             NOT NULL,
    Token       VARCHAR(512)    NOT NULL,
    ExpiresAt   DATETIME2       NOT NULL,
    IsRevoked   BIT             NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_RefreshTokens PRIMARY KEY (TokenId),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- ============================================================
-- SECTION 2: WAREHOUSE STRUCTURE (5 levels)
-- ============================================================

CREATE TABLE Warehouses (
    WarehouseId     INT             NOT NULL IDENTITY(1,1),
    Code            VARCHAR(20)     NOT NULL,
    Name            NVARCHAR(150)   NOT NULL,
    NameEn          NVARCHAR(150)   NULL,
    Address         NVARCHAR(500)   NULL,
    ManagerUserId   INT             NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    CONSTRAINT PK_Warehouses PRIMARY KEY (WarehouseId),
    CONSTRAINT UQ_Warehouses_Code UNIQUE (Code),
    CONSTRAINT FK_Warehouses_Manager FOREIGN KEY (ManagerUserId) REFERENCES Users(UserId)
);
GO

CREATE TABLE Zones (
    ZoneId          INT             NOT NULL IDENTITY(1,1),
    WarehouseId     INT             NOT NULL,
    Code            VARCHAR(20)     NOT NULL,
    Name            NVARCHAR(150)   NOT NULL,
    NameEn          NVARCHAR(150)   NULL,
    ZoneType        VARCHAR(50)     NULL,   -- COLD, DRY, HAZMAT, GENERAL ...
    Description     NVARCHAR(500)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Zones PRIMARY KEY (ZoneId),
    CONSTRAINT UQ_Zones_Code UNIQUE (WarehouseId, Code),
    CONSTRAINT FK_Zones_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId)
);
GO

CREATE TABLE Racks (
    RackId      INT             NOT NULL IDENTITY(1,1),
    ZoneId      INT             NOT NULL,
    Code        VARCHAR(20)     NOT NULL,
    Name        NVARCHAR(100)   NOT NULL,
    IsActive    BIT             NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Racks PRIMARY KEY (RackId),
    CONSTRAINT UQ_Racks_Code UNIQUE (ZoneId, Code),
    CONSTRAINT FK_Racks_Zones FOREIGN KEY (ZoneId) REFERENCES Zones(ZoneId)
);
GO

CREATE TABLE Shelves (
    ShelfId     INT             NOT NULL IDENTITY(1,1),
    RackId      INT             NOT NULL,
    Code        VARCHAR(20)     NOT NULL,
    Name        NVARCHAR(100)   NOT NULL,
    IsActive    BIT             NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Shelves PRIMARY KEY (ShelfId),
    CONSTRAINT UQ_Shelves_Code UNIQUE (RackId, Code),
    CONSTRAINT FK_Shelves_Racks FOREIGN KEY (RackId) REFERENCES Racks(RackId)
);
GO

CREATE TABLE Bins (
    BinId           INT             NOT NULL IDENTITY(1,1),
    ShelfId         INT             NOT NULL,
    Code            VARCHAR(30)     NOT NULL,
    Name            NVARCHAR(100)   NOT NULL,
    MaxCapacity     DECIMAL(18,4)   NULL,   -- optional capacity limit
    CapacityUnit    VARCHAR(20)     NULL,   -- KG, CBM, UNIT ...
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Bins PRIMARY KEY (BinId),
    CONSTRAINT UQ_Bins_Code UNIQUE (ShelfId, Code),
    CONSTRAINT FK_Bins_Shelves FOREIGN KEY (ShelfId) REFERENCES Shelves(ShelfId)
);
GO

-- View: Full location path for a Bin
CREATE VIEW vw_BinFullPath AS
SELECT
    b.BinId,
    b.Code          AS BinCode,
    s.Code          AS ShelfCode,
    r.Code          AS RackCode,
    z.Code          AS ZoneCode,
    w.Code          AS WarehouseCode,
    w.Name          AS WarehouseName,
    CONCAT(w.Code, '-', z.Code, '-', r.Code, '-', s.Code, '-', b.Code) AS FullPath
FROM Bins b
JOIN Shelves s ON s.ShelfId = b.ShelfId
JOIN Racks r   ON r.RackId  = s.RackId
JOIN Zones z   ON z.ZoneId  = r.ZoneId
JOIN Warehouses w ON w.WarehouseId = z.WarehouseId;
GO

-- ============================================================
-- SECTION 3: PRODUCT CATALOG
-- ============================================================

CREATE TABLE ProductCategories (
    CategoryId      INT             NOT NULL IDENTITY(1,1),
    ParentId        INT             NULL,   -- Self-referencing for tree
    Code            VARCHAR(20)     NOT NULL,
    Name            NVARCHAR(150)   NOT NULL,
    NameEn          NVARCHAR(150)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ProductCategories PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_ProductCategories_Code UNIQUE (Code),
    CONSTRAINT FK_ProductCategories_Parent FOREIGN KEY (ParentId) REFERENCES ProductCategories(CategoryId)
);
GO

CREATE TABLE UnitOfMeasures (
    UomId       INT             NOT NULL IDENTITY(1,1),
    Code        VARCHAR(20)     NOT NULL,
    Name        NVARCHAR(100)   NOT NULL,
    NameEn      NVARCHAR(100)   NULL,
    IsActive    BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_UnitOfMeasures PRIMARY KEY (UomId),
    CONSTRAINT UQ_UnitOfMeasures_Code UNIQUE (Code)
);
GO

CREATE TABLE Products (
    ProductId           INT             NOT NULL IDENTITY(1,1),
    SKU                 VARCHAR(50)     NOT NULL,
    Barcode             VARCHAR(100)    NULL,
    Name                NVARCHAR(200)   NOT NULL,
    NameEn              NVARCHAR(200)   NULL,
    CategoryId          INT             NOT NULL,
    UomId               INT             NOT NULL,
    Description         NVARCHAR(2000)  NULL,
    ImageUrl            NVARCHAR(500)   NULL,
    -- Stock control
    MinStock            DECIMAL(18,4)   NOT NULL DEFAULT 0,
    ReorderPoint        DECIMAL(18,4)   NOT NULL DEFAULT 0,
    -- Batch config
    IsBatchTracked      BIT             NOT NULL DEFAULT 1,
    IsExpiryTracked     BIT             NOT NULL DEFAULT 0,
    ExpiryWarningDays   INT             NOT NULL DEFAULT 30,  -- Alert N days before expiry
    -- Status
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NULL,
    CreatedBy           INT             NOT NULL,
    CONSTRAINT PK_Products PRIMARY KEY (ProductId),
    CONSTRAINT UQ_Products_SKU UNIQUE (SKU),
    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES ProductCategories(CategoryId),
    CONSTRAINT FK_Products_UOM FOREIGN KEY (UomId) REFERENCES UnitOfMeasures(UomId),
    CONSTRAINT FK_Products_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
GO

-- ============================================================
-- SECTION 4: SUPPLIERS & CUSTOMERS
-- ============================================================

CREATE TABLE Suppliers (
    SupplierId          INT             NOT NULL IDENTITY(1,1),
    Code                VARCHAR(20)     NOT NULL,
    Name                NVARCHAR(200)   NOT NULL,
    TaxCode             VARCHAR(20)     NULL,
    Address             NVARCHAR(500)   NULL,
    Email               NVARCHAR(200)   NULL,
    Phone               VARCHAR(30)     NULL,
    Website             NVARCHAR(300)   NULL,
    ContactPerson       NVARCHAR(150)   NULL,
    ContactPhone        VARCHAR(30)     NULL,
    -- Contract info
    ContractNumber      VARCHAR(50)     NULL,
    ContractStartDate   DATE            NULL,
    ContractEndDate     DATE            NULL,
    PaymentTerms        NVARCHAR(300)   NULL,
    Notes               NVARCHAR(1000)  NULL,
    -- Status: Active | Inactive | Blacklisted
    Status              VARCHAR(20)     NOT NULL DEFAULT 'Active',
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NULL,
    CONSTRAINT PK_Suppliers PRIMARY KEY (SupplierId),
    CONSTRAINT UQ_Suppliers_Code UNIQUE (Code),
    CONSTRAINT CK_Suppliers_Status CHECK (Status IN ('Active','Inactive','Blacklisted'))
);
GO

CREATE TABLE Customers (
    CustomerId          INT             NOT NULL IDENTITY(1,1),
    Code                VARCHAR(20)     NOT NULL,
    Name                NVARCHAR(200)   NOT NULL,
    -- B2B_SERVICE: khach thue luu kho | CONSIGNEE: nguoi nhan hang
    CustomerType        VARCHAR(20)     NOT NULL DEFAULT 'CONSIGNEE',
    TaxCode             VARCHAR(20)     NULL,
    Address             NVARCHAR(500)   NULL,
    Email               NVARCHAR(200)   NULL,
    Phone               VARCHAR(30)     NULL,
    ContactPerson       NVARCHAR(150)   NULL,
    ContactPhone        VARCHAR(30)     NULL,
    -- Contract (for B2B_SERVICE)
    ContractNumber      VARCHAR(50)     NULL,
    ContractStartDate   DATE            NULL,
    ContractEndDate     DATE            NULL,
    ServiceTerms        NVARCHAR(500)   NULL,
    Notes               NVARCHAR(1000)  NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NULL,
    CONSTRAINT PK_Customers PRIMARY KEY (CustomerId),
    CONSTRAINT UQ_Customers_Code UNIQUE (Code),
    CONSTRAINT CK_Customers_Type CHECK (CustomerType IN ('B2B_SERVICE','CONSIGNEE'))
);
GO

-- ============================================================
-- SECTION 5: BATCH / LOT MANAGEMENT
-- ============================================================

CREATE TABLE Batches (
    BatchId         INT             NOT NULL IDENTITY(1,1),
    ProductId       INT             NOT NULL,
    SupplierId      INT             NULL,
    LotNumber       VARCHAR(100)    NOT NULL,
    MfgDate         DATE            NULL,
    ExpiryDate      DATE            NULL,
    InitialQty      DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Notes           NVARCHAR(500)   NULL,
    -- Status: ACTIVE | EXPIRED | CONSUMED | RECALLED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'ACTIVE',
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy       INT             NOT NULL,
    CONSTRAINT PK_Batches PRIMARY KEY (BatchId),
    CONSTRAINT FK_Batches_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_Batches_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT FK_Batches_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_Batches_Status CHECK (Status IN ('ACTIVE','EXPIRED','CONSUMED','RECALLED'))
);
GO

-- *** CORE STOCK TABLE: Current stock per Bin per Batch ***
CREATE TABLE BinStocks (
    BinStockId      INT             NOT NULL IDENTITY(1,1),
    BinId           INT             NOT NULL,
    BatchId         INT             NOT NULL,
    ProductId       INT             NOT NULL,   -- Denormalized for query perf
    Quantity        DECIMAL(18,4)   NOT NULL DEFAULT 0,
    ReservedQty     DECIMAL(18,4)   NOT NULL DEFAULT 0,   -- Qty reserved by pending GDN
    RowVersion      ROWVERSION,                            -- Optimistic concurrency
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BinStocks PRIMARY KEY (BinStockId),
    CONSTRAINT UQ_BinStocks UNIQUE (BinId, BatchId),
    CONSTRAINT FK_BinStocks_Bins FOREIGN KEY (BinId) REFERENCES Bins(BinId),
    CONSTRAINT FK_BinStocks_Batches FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_BinStocks_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT CK_BinStocks_Qty CHECK (Quantity >= 0),
    CONSTRAINT CK_BinStocks_Reserved CHECK (ReservedQty >= 0)
);
GO

-- View: Total stock per Product (rollup from BinStocks)
CREATE VIEW vw_ProductStock AS
SELECT
    bs.ProductId,
    p.SKU,
    p.Name,
    SUM(bs.Quantity)        AS TotalQty,
    SUM(bs.ReservedQty)     AS TotalReserved,
    SUM(bs.Quantity - bs.ReservedQty) AS AvailableQty,
    p.MinStock,
    p.ReorderPoint,
    CASE WHEN SUM(bs.Quantity - bs.ReservedQty) <= p.MinStock THEN 1 ELSE 0 END AS IsBelowMinStock
FROM BinStocks bs
JOIN Products p ON p.ProductId = bs.ProductId
WHERE bs.Quantity > 0
GROUP BY bs.ProductId, p.SKU, p.Name, p.MinStock, p.ReorderPoint;
GO

-- FEFO View: Batches ordered by ExpiryDate for dispatch suggestion
CREATE VIEW vw_FEFOStock AS
SELECT
    bs.BinId,
    bs.BatchId,
    bs.ProductId,
    b.LotNumber,
    b.ExpiryDate,
    b.MfgDate,
    (bs.Quantity - bs.ReservedQty) AS AvailableQty,
    fp.FullPath AS BinPath,
    ROW_NUMBER() OVER (
        PARTITION BY bs.ProductId
        ORDER BY
            CASE WHEN b.ExpiryDate IS NULL THEN 1 ELSE 0 END,  -- nulls last
            b.ExpiryDate ASC,
            bs.BatchId ASC
    ) AS FEFOPriority
FROM BinStocks bs
JOIN Batches b ON b.BatchId = bs.BatchId
JOIN vw_BinFullPath fp ON fp.BinId = bs.BinId
WHERE bs.Quantity - bs.ReservedQty > 0
  AND b.Status = 'ACTIVE';
GO

-- ============================================================
-- SECTION 6: APPROVAL ENGINE (Generic / Polymorphic)
-- ============================================================

CREATE TABLE ApprovalWorkflows (
    WorkflowId      INT             NOT NULL IDENTITY(1,1),
    -- DocumentType: GRN | GDN | TRANSFER | STOCK_ADJUSTMENT
    DocumentType    VARCHAR(30)     NOT NULL,
    Level           TINYINT         NOT NULL,   -- 1 = L1 Manager, 2 = L2 Director
    ApproverRoleCode VARCHAR(30)    NOT NULL,
    Description     NVARCHAR(200)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_ApprovalWorkflows PRIMARY KEY (WorkflowId),
    CONSTRAINT UQ_ApprovalWorkflows UNIQUE (DocumentType, Level)
);
GO

-- Pre-populate approval workflow (2-level fixed for all documents)
INSERT INTO ApprovalWorkflows (DocumentType, Level, ApproverRoleCode, Description) VALUES
('GRN',              1, 'WH_MANAGER', 'GRN Level 1 - Warehouse Manager'),
('GRN',              2, 'DIRECTOR',   'GRN Level 2 - Director'),
('GDN',              1, 'WH_MANAGER', 'GDN Level 1 - Warehouse Manager'),
('GDN',              2, 'DIRECTOR',   'GDN Level 2 - Director'),
('TRANSFER',         1, 'WH_MANAGER', 'Transfer Level 1 - Warehouse Manager'),
('TRANSFER',         2, 'DIRECTOR',   'Transfer Level 2 - Director'),
('STOCK_ADJUSTMENT', 1, 'WH_MANAGER', 'Adjustment Level 1 - Warehouse Manager'),
('STOCK_ADJUSTMENT', 2, 'DIRECTOR',   'Adjustment Level 2 - Director');
GO

CREATE TABLE Approvals (
    ApprovalId      INT             NOT NULL IDENTITY(1,1),
    DocumentType    VARCHAR(30)     NOT NULL,
    DocumentId      INT             NOT NULL,
    Level           TINYINT         NOT NULL,
    -- Status: PENDING | APPROVED | REJECTED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'PENDING',
    ApproverId      INT             NULL,
    ApprovedAt      DATETIME2       NULL,
    Comment         NVARCHAR(1000)  NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Approvals PRIMARY KEY (ApprovalId),
    CONSTRAINT FK_Approvals_Approver FOREIGN KEY (ApproverId) REFERENCES Users(UserId),
    CONSTRAINT CK_Approvals_Status CHECK (Status IN ('PENDING','APPROVED','REJECTED'))
);

CREATE INDEX IX_Approvals_Document ON Approvals (DocumentType, DocumentId);
CREATE INDEX IX_Approvals_Pending  ON Approvals (Status, DocumentType) WHERE Status = 'PENDING';
GO

-- ============================================================
-- SECTION 7: PURCHASE ORDERS
-- ============================================================

CREATE TABLE PurchaseOrders (
    POId            INT             NOT NULL IDENTITY(1,1),
    PONumber        VARCHAR(30)     NOT NULL,   -- PO-YYYYMM-XXXXX
    SupplierId      INT             NOT NULL,
    WarehouseId     INT             NOT NULL,
    OrderDate       DATE            NOT NULL DEFAULT CAST(GETUTCDATE() AS DATE),
    ExpectedDate    DATE            NULL,
    Notes           NVARCHAR(1000)  NULL,
    -- Status: DRAFT | SUBMITTED | PARTIALLY_RECEIVED | RECEIVED | CANCELLED
    Status          VARCHAR(30)     NOT NULL DEFAULT 'DRAFT',
    CreatedBy       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    CONSTRAINT PK_PurchaseOrders PRIMARY KEY (POId),
    CONSTRAINT UQ_PurchaseOrders_Number UNIQUE (PONumber),
    CONSTRAINT FK_PurchaseOrders_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT FK_PurchaseOrders_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_PurchaseOrders_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
GO

CREATE TABLE POLines (
    POLineId        INT             NOT NULL IDENTITY(1,1),
    POId            INT             NOT NULL,
    ProductId       INT             NOT NULL,
    OrderedQty      DECIMAL(18,4)   NOT NULL,
    ReceivedQty     DECIMAL(18,4)   NOT NULL DEFAULT 0,
    UnitPrice       DECIMAL(18,4)   NULL,
    Notes           NVARCHAR(500)   NULL,
    CONSTRAINT PK_POLines PRIMARY KEY (POLineId),
    CONSTRAINT FK_POLines_PO FOREIGN KEY (POId) REFERENCES PurchaseOrders(POId),
    CONSTRAINT FK_POLines_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT CK_POLines_OrderedQty CHECK (OrderedQty > 0)
);
GO

-- ============================================================
-- SECTION 8: GOODS RECEIPT NOTES (GRN)
-- ============================================================

CREATE TABLE GoodsReceiptNotes (
    GRNId           INT             NOT NULL IDENTITY(1,1),
    GRNNumber       VARCHAR(30)     NOT NULL,   -- GRN-YYYYMM-XXXXX
    POId            INT             NULL,
    SupplierId      INT             NOT NULL,
    WarehouseId     INT             NOT NULL,
    ReceiptDate     DATE            NOT NULL DEFAULT CAST(GETUTCDATE() AS DATE),
    Notes           NVARCHAR(1000)  NULL,
    AttachmentUrls  NVARCHAR(MAX)   NULL,   -- JSON array of file paths
    -- Status: DRAFT | PENDING_L1 | PENDING_L2 | APPROVED | REJECTED | CANCELLED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'DRAFT',
    -- Return flag
    IsReturn        BIT             NOT NULL DEFAULT 0,
    ParentGRNId     INT             NULL,   -- For return: link to original GRN
    CreatedBy       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    CompletedAt     DATETIME2       NULL,
    CONSTRAINT PK_GoodsReceiptNotes PRIMARY KEY (GRNId),
    CONSTRAINT UQ_GRN_Number UNIQUE (GRNNumber),
    CONSTRAINT FK_GRN_PO FOREIGN KEY (POId) REFERENCES PurchaseOrders(POId),
    CONSTRAINT FK_GRN_Supplier FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT FK_GRN_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_GRN_ParentGRN FOREIGN KEY (ParentGRNId) REFERENCES GoodsReceiptNotes(GRNId),
    CONSTRAINT FK_GRN_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_GRN_Status CHECK (Status IN ('DRAFT','PENDING_L1','PENDING_L2','APPROVED','REJECTED','CANCELLED'))
);
GO

CREATE TABLE GRNLines (
    GRNLineId       INT             NOT NULL IDENTITY(1,1),
    GRNId           INT             NOT NULL,
    ProductId       INT             NOT NULL,
    BatchId         INT             NULL,   -- Populated after batch is created
    BinId           INT             NOT NULL,
    Quantity        DECIMAL(18,4)   NOT NULL,
    LotNumber       VARCHAR(100)    NULL,
    MfgDate         DATE            NULL,
    ExpiryDate      DATE            NULL,
    UnitPrice       DECIMAL(18,4)   NULL,
    Notes           NVARCHAR(500)   NULL,
    CONSTRAINT PK_GRNLines PRIMARY KEY (GRNLineId),
    CONSTRAINT FK_GRNLines_GRN FOREIGN KEY (GRNId) REFERENCES GoodsReceiptNotes(GRNId),
    CONSTRAINT FK_GRNLines_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_GRNLines_Batches FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_GRNLines_Bins FOREIGN KEY (BinId) REFERENCES Bins(BinId),
    CONSTRAINT CK_GRNLines_Qty CHECK (Quantity > 0)
);
GO

-- ============================================================
-- SECTION 9: GOODS DISPATCH NOTES (GDN)
-- ============================================================

CREATE TABLE DispatchRequests (
    RequestId       INT             NOT NULL IDENTITY(1,1),
    RequestNumber   VARCHAR(30)     NOT NULL,
    CustomerId      INT             NOT NULL,
    WarehouseId     INT             NOT NULL,
    RequestDate     DATE            NOT NULL DEFAULT CAST(GETUTCDATE() AS DATE),
    RequiredDate    DATE            NULL,
    Notes           NVARCHAR(1000)  NULL,
    -- Status: OPEN | IN_PROGRESS | FULFILLED | CANCELLED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'OPEN',
    CreatedBy       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DispatchRequests PRIMARY KEY (RequestId),
    CONSTRAINT UQ_DispatchRequests_Number UNIQUE (RequestNumber),
    CONSTRAINT FK_DispatchRequests_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_DispatchRequests_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_DispatchRequests_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
GO

CREATE TABLE GoodsDispatchNotes (
    GDNId           INT             NOT NULL IDENTITY(1,1),
    GDNNumber       VARCHAR(30)     NOT NULL,   -- GDN-YYYYMM-XXXXX
    RequestId       INT             NULL,
    CustomerId      INT             NOT NULL,
    WarehouseId     INT             NOT NULL,
    DispatchDate    DATE            NOT NULL DEFAULT CAST(GETUTCDATE() AS DATE),
    DeliveryAddress NVARCHAR(500)   NULL,
    Notes           NVARCHAR(1000)  NULL,
    AttachmentUrls  NVARCHAR(MAX)   NULL,
    -- Status: DRAFT | PENDING_L1 | PENDING_L2 | APPROVED | PICKING | PICKED | DELIVERED | CANCELLED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'DRAFT',
    -- Return flag
    IsReturn        BIT             NOT NULL DEFAULT 0,
    ParentGDNId     INT             NULL,
    PickedAt        DATETIME2       NULL,
    PickedBy        INT             NULL,
    DeliveredAt     DATETIME2       NULL,
    DeliveredBy     INT             NULL,
    CreatedBy       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    CONSTRAINT PK_GoodsDispatchNotes PRIMARY KEY (GDNId),
    CONSTRAINT UQ_GDN_Number UNIQUE (GDNNumber),
    CONSTRAINT FK_GDN_Request FOREIGN KEY (RequestId) REFERENCES DispatchRequests(RequestId),
    CONSTRAINT FK_GDN_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_GDN_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_GDN_ParentGDN FOREIGN KEY (ParentGDNId) REFERENCES GoodsDispatchNotes(GDNId),
    CONSTRAINT FK_GDN_PickedBy FOREIGN KEY (PickedBy) REFERENCES Users(UserId),
    CONSTRAINT FK_GDN_DeliveredBy FOREIGN KEY (DeliveredBy) REFERENCES Users(UserId),
    CONSTRAINT FK_GDN_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_GDN_Status CHECK (Status IN ('DRAFT','PENDING_L1','PENDING_L2','APPROVED','PICKING','PICKED','DELIVERED','CANCELLED'))
);
GO

CREATE TABLE GDNLines (
    GDNLineId       INT             NOT NULL IDENTITY(1,1),
    GDNId           INT             NOT NULL,
    ProductId       INT             NOT NULL,
    BatchId         INT             NOT NULL,   -- FEFO-selected batch
    BinId           INT             NOT NULL,
    RequestedQty    DECIMAL(18,4)   NOT NULL,
    PickedQty       DECIMAL(18,4)   NULL,       -- Actual picked qty
    Notes           NVARCHAR(500)   NULL,
    CONSTRAINT PK_GDNLines PRIMARY KEY (GDNLineId),
    CONSTRAINT FK_GDNLines_GDN FOREIGN KEY (GDNId) REFERENCES GoodsDispatchNotes(GDNId),
    CONSTRAINT FK_GDNLines_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_GDNLines_Batches FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_GDNLines_Bins FOREIGN KEY (BinId) REFERENCES Bins(BinId),
    CONSTRAINT CK_GDNLines_Qty CHECK (RequestedQty > 0)
);
GO

-- ============================================================
-- SECTION 10: INTERNAL TRANSFER ORDERS
-- ============================================================

CREATE TABLE TransferOrders (
    TransferId      INT             NOT NULL IDENTITY(1,1),
    TransferNumber  VARCHAR(30)     NOT NULL,   -- TO-YYYYMM-XXXXX
    WarehouseId     INT             NOT NULL,
    TransferDate    DATE            NOT NULL DEFAULT CAST(GETUTCDATE() AS DATE),
    Reason          NVARCHAR(500)   NULL,
    -- Status: DRAFT | PENDING_L1 | PENDING_L2 | APPROVED | COMPLETED | CANCELLED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'DRAFT',
    CreatedBy       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    CompletedAt     DATETIME2       NULL,
    CONSTRAINT PK_TransferOrders PRIMARY KEY (TransferId),
    CONSTRAINT UQ_TransferOrders_Number UNIQUE (TransferNumber),
    CONSTRAINT FK_TransferOrders_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_TransferOrders_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_TransferOrders_Status CHECK (Status IN ('DRAFT','PENDING_L1','PENDING_L2','APPROVED','COMPLETED','CANCELLED'))
);
GO

CREATE TABLE TransferOrderLines (
    TransferLineId  INT             NOT NULL IDENTITY(1,1),
    TransferId      INT             NOT NULL,
    ProductId       INT             NOT NULL,
    BatchId         INT             NOT NULL,
    FromBinId       INT             NOT NULL,
    ToBinId         INT             NOT NULL,
    Quantity        DECIMAL(18,4)   NOT NULL,
    Notes           NVARCHAR(500)   NULL,
    CONSTRAINT PK_TransferOrderLines PRIMARY KEY (TransferLineId),
    CONSTRAINT FK_TransferLines_Transfer FOREIGN KEY (TransferId) REFERENCES TransferOrders(TransferId),
    CONSTRAINT FK_TransferLines_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_TransferLines_Batches FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_TransferLines_FromBin FOREIGN KEY (FromBinId) REFERENCES Bins(BinId),
    CONSTRAINT FK_TransferLines_ToBin FOREIGN KEY (ToBinId) REFERENCES Bins(BinId),
    CONSTRAINT CK_TransferLines_Qty CHECK (Quantity > 0),
    CONSTRAINT CK_TransferLines_DiffBin CHECK (FromBinId <> ToBinId)
);
GO

-- ============================================================
-- SECTION 11: STOCK COUNT (Inventory)
-- ============================================================

CREATE TABLE StockCounts (
    CountId         INT             NOT NULL IDENTITY(1,1),
    CountNumber     VARCHAR(30)     NOT NULL,   -- SC-YYYYMM-XXXXX
    WarehouseId     INT             NOT NULL,
    -- CountType: FULL | PERIODIC | CYCLE
    CountType       VARCHAR(20)     NOT NULL,
    -- Scope
    ZoneId          INT             NULL,   -- NULL = full warehouse
    RackId          INT             NULL,
    CountDate       DATE            NOT NULL,
    PlannedBy       INT             NOT NULL,
    Notes           NVARCHAR(1000)  NULL,
    -- Status: PLANNED | IN_PROGRESS | COMPLETED | ADJUSTMENT_PENDING | CLOSED
    Status          VARCHAR(30)     NOT NULL DEFAULT 'PLANNED',
    StartedAt       DATETIME2       NULL,
    CompletedAt     DATETIME2       NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_StockCounts PRIMARY KEY (CountId),
    CONSTRAINT UQ_StockCounts_Number UNIQUE (CountNumber),
    CONSTRAINT FK_StockCounts_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_StockCounts_Zone FOREIGN KEY (ZoneId) REFERENCES Zones(ZoneId),
    CONSTRAINT FK_StockCounts_Rack FOREIGN KEY (RackId) REFERENCES Racks(RackId),
    CONSTRAINT FK_StockCounts_PlannedBy FOREIGN KEY (PlannedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_StockCounts_Type CHECK (CountType IN ('FULL','PERIODIC','CYCLE'))
);
GO

CREATE TABLE StockCountLines (
    CountLineId         INT             NOT NULL IDENTITY(1,1),
    CountId             INT             NOT NULL,
    BinId               INT             NOT NULL,
    ProductId           INT             NOT NULL,
    BatchId             INT             NOT NULL,
    SystemQty           DECIMAL(18,4)   NOT NULL,   -- Snapshot at count time
    ActualQty           DECIMAL(18,4)   NULL,       -- Filled by WH_STAFF
    Variance            AS (ISNULL(ActualQty, 0) - SystemQty) PERSISTED,
    CountedBy           INT             NULL,
    CountedAt           DATETIME2       NULL,
    Notes               NVARCHAR(500)   NULL,
    CONSTRAINT PK_StockCountLines PRIMARY KEY (CountLineId),
    CONSTRAINT FK_CountLines_Count FOREIGN KEY (CountId) REFERENCES StockCounts(CountId),
    CONSTRAINT FK_CountLines_Bin FOREIGN KEY (BinId) REFERENCES Bins(BinId),
    CONSTRAINT FK_CountLines_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_CountLines_Batch FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_CountLines_CountedBy FOREIGN KEY (CountedBy) REFERENCES Users(UserId)
);
GO

-- ============================================================
-- SECTION 12: STOCK ADJUSTMENTS
-- ============================================================

CREATE TABLE StockAdjustments (
    AdjustmentId    INT             NOT NULL IDENTITY(1,1),
    AdjNumber       VARCHAR(30)     NOT NULL,   -- ADJ-YYYYMM-XXXXX
    CountId         INT             NULL,   -- Link to StockCount if from inventory
    WarehouseId     INT             NOT NULL,
    Reason          NVARCHAR(500)   NOT NULL,
    Notes           NVARCHAR(1000)  NULL,
    -- Status: DRAFT | PENDING_L1 | PENDING_L2 | APPROVED | REJECTED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'DRAFT',
    CreatedBy       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    ApprovedAt      DATETIME2       NULL,
    CONSTRAINT PK_StockAdjustments PRIMARY KEY (AdjustmentId),
    CONSTRAINT UQ_StockAdjustments_Number UNIQUE (AdjNumber),
    CONSTRAINT FK_Adj_Count FOREIGN KEY (CountId) REFERENCES StockCounts(CountId),
    CONSTRAINT FK_Adj_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_Adj_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
GO

CREATE TABLE StockAdjustmentLines (
    AdjLineId       INT             NOT NULL IDENTITY(1,1),
    AdjustmentId    INT             NOT NULL,
    BinId           INT             NOT NULL,
    ProductId       INT             NOT NULL,
    BatchId         INT             NOT NULL,
    BeforeQty       DECIMAL(18,4)   NOT NULL,
    AfterQty        DECIMAL(18,4)   NOT NULL,
    DeltaQty        AS (AfterQty - BeforeQty) PERSISTED,
    Notes           NVARCHAR(500)   NULL,
    CONSTRAINT PK_StockAdjustmentLines PRIMARY KEY (AdjLineId),
    CONSTRAINT FK_AdjLines_Adj FOREIGN KEY (AdjustmentId) REFERENCES StockAdjustments(AdjustmentId),
    CONSTRAINT FK_AdjLines_Bin FOREIGN KEY (BinId) REFERENCES Bins(BinId),
    CONSTRAINT FK_AdjLines_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_AdjLines_Batch FOREIGN KEY (BatchId) REFERENCES Batches(BatchId)
);
GO

-- ============================================================
-- SECTION 13: STOCK TRANSACTION LEDGER (Audit trail of movements)
-- ============================================================

CREATE TABLE StockTransactions (
    TxnId           BIGINT          NOT NULL IDENTITY(1,1),
    ProductId       INT             NOT NULL,
    BatchId         INT             NOT NULL,
    BinId           INT             NOT NULL,
    -- TxnType: GRN_IN | GRN_RETURN_OUT | GDN_OUT | GDN_RETURN_IN | TRANSFER_OUT | TRANSFER_IN | ADJ_IN | ADJ_OUT
    TxnType         VARCHAR(30)     NOT NULL,
    DocumentType    VARCHAR(20)     NOT NULL,   -- GRN | GDN | TRANSFER | ADJ
    DocumentId      INT             NOT NULL,
    DocumentNumber  VARCHAR(30)     NOT NULL,
    Quantity        DECIMAL(18,4)   NOT NULL,   -- always positive; direction in TxnType
    QtyBefore       DECIMAL(18,4)   NOT NULL,
    QtyAfter        DECIMAL(18,4)   NOT NULL,
    Remarks         NVARCHAR(500)   NULL,
    CreatedBy       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_StockTransactions PRIMARY KEY (TxnId),
    CONSTRAINT FK_Txn_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_Txn_Batch FOREIGN KEY (BatchId) REFERENCES Batches(BatchId),
    CONSTRAINT FK_Txn_Bin FOREIGN KEY (BinId) REFERENCES Bins(BinId),
    CONSTRAINT FK_Txn_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

CREATE INDEX IX_StockTxn_Product_Date ON StockTransactions (ProductId, CreatedAt);
CREATE INDEX IX_StockTxn_Batch ON StockTransactions (BatchId);
CREATE INDEX IX_StockTxn_Document ON StockTransactions (DocumentType, DocumentId);
GO

-- ============================================================
-- SECTION 14: NOTIFICATIONS
-- ============================================================

CREATE TABLE Notifications (
    NotificationId  INT             NOT NULL IDENTITY(1,1),
    UserId          INT             NOT NULL,
    -- Type: LOW_STOCK | EXPIRY_WARNING | EXPIRED | APPROVAL_PENDING | APPROVAL_RESULT | COUNT_VARIANCE | DELIVERY_CONFIRM
    NotifType       VARCHAR(40)     NOT NULL,
    Title           NVARCHAR(200)   NOT NULL,
    TitleEn         NVARCHAR(200)   NULL,
    Body            NVARCHAR(2000)  NOT NULL,
    BodyEn          NVARCHAR(2000)  NULL,
    ReferenceType   VARCHAR(20)     NULL,   -- GRN|GDN|TRANSFER|PRODUCT|BATCH
    ReferenceId     INT             NULL,
    IsRead          BIT             NOT NULL DEFAULT 0,
    ReadAt          DATETIME2       NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Notifications PRIMARY KEY (NotificationId),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_Notifications_User_Unread ON Notifications (UserId, IsRead) WHERE IsRead = 0;
GO

CREATE TABLE EmailLogs (
    EmailLogId      INT             NOT NULL IDENTITY(1,1),
    RecipientEmail  NVARCHAR(200)   NOT NULL,
    Subject         NVARCHAR(500)   NOT NULL,
    TemplateName    VARCHAR(100)    NULL,
    ReferenceType   VARCHAR(20)     NULL,
    ReferenceId     INT             NULL,
    -- Status: QUEUED | SENT | FAILED
    Status          VARCHAR(20)     NOT NULL DEFAULT 'QUEUED',
    SentAt          DATETIME2       NULL,
    RetryCount      TINYINT         NOT NULL DEFAULT 0,
    ErrorMessage    NVARCHAR(1000)  NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_EmailLogs PRIMARY KEY (EmailLogId)
);
GO

-- ============================================================
-- SECTION 15: SYSTEM CONFIGURATION
-- ============================================================

CREATE TABLE SystemSettings (
    SettingId       INT             NOT NULL IDENTITY(1,1),
    SettingKey      VARCHAR(100)    NOT NULL,
    SettingValue    NVARCHAR(2000)  NULL,
    DataType        VARCHAR(20)     NOT NULL DEFAULT 'string',  -- string | int | bool | json
    Description     NVARCHAR(500)   NULL,
    UpdatedAt       DATETIME2       NULL,
    UpdatedBy       INT             NULL,
    CONSTRAINT PK_SystemSettings PRIMARY KEY (SettingId),
    CONSTRAINT UQ_SystemSettings_Key UNIQUE (SettingKey)
);
GO

-- Default settings
INSERT INTO SystemSettings (SettingKey, SettingValue, DataType, Description) VALUES
('smtp.host',               'smtp.gmail.com',   'string',   'SMTP server host'),
('smtp.port',               '587',              'int',      'SMTP port'),
('smtp.username',           '',                 'string',   'SMTP username'),
('smtp.password',           '',                 'string',   'SMTP password (encrypted)'),
('smtp.fromEmail',          '',                 'string',   'From email address'),
('smtp.fromName',           'WMS System',       'string',   'From display name'),
('alert.expiryWarningDays', '30',               'int',      'Days before expiry to alert'),
('alert.countVariancePct',  '5',                'int',      'Stock count variance % to trigger alert'),
('system.defaultLang',      'vi',               'string',   'Default UI language'),
('system.pageSize',         '20',               'int',      'Default pagination page size');
GO

-- ============================================================
-- SECTION 16: SEED DATA
-- ============================================================

-- Roles
INSERT INTO Roles (RoleCode, RoleName, RoleNameEn) VALUES
('SYS_ADMIN',   N'Quan tri he thong',       'System Administrator'),
('DIRECTOR',    N'Giam doc',                 'Director'),
('WH_MANAGER',  N'Truong kho',              'Warehouse Manager'),
('WH_STAFF',    N'Nhan vien kho',            'Warehouse Staff'),
('INV_CTRL',    N'Kiem soat ton kho',       'Inventory Controller'),
('PURCHASING',  N'Nhan vien mua hang',       'Purchasing Staff'),
('SALES',       N'Nhan vien kinh doanh',     'Sales Staff');
GO

-- Default Admin user (password: Admin@123 -- CHANGE IN PRODUCTION)
-- BCrypt hash placeholder -- replace with actual hash generated by application
INSERT INTO Users (Username, Email, PasswordHash, FullName, FullNameEn, PreferredLang)
VALUES ('admin', 'admin@wms.local',
        '$2a$12$PLACEHOLDER_HASH_REPLACE_ME',
        N'Quan tri vien he thong', 'System Administrator', 'vi');

INSERT INTO UserRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM Users u, Roles r
WHERE u.Username = 'admin' AND r.RoleCode = 'SYS_ADMIN';
GO

-- Default UoMs
INSERT INTO UnitOfMeasures (Code, Name, NameEn) VALUES
('PCS',     N'Cai',         'Piece'),
('KG',      N'Kilogram',    'Kilogram'),
('BOX',     N'Thung',       'Box'),
('PLT',     N'Pallet',      'Pallet'),
('BAG',     N'Tui',         'Bag'),
('CBM',     N'Met khoi',    'Cubic Meter');
GO

-- ============================================================
-- SECTION 17: STORED PROCEDURES
-- ============================================================

-- SP: Generate document number with format PREFIX-YYYYMM-XXXXX
CREATE OR ALTER PROCEDURE sp_GenerateDocNumber
    @Prefix     VARCHAR(10),
    @TableName  VARCHAR(50),
    @NumberColumn VARCHAR(50),
    @Result     VARCHAR(30) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @YearMonth VARCHAR(6) = FORMAT(GETUTCDATE(), 'yyyyMM');
    DECLARE @Pattern VARCHAR(20) = @Prefix + '-' + @YearMonth + '-%';
    DECLARE @MaxNum INT;

    EXEC sp_executesql
        N'SELECT @MaxNum = ISNULL(MAX(CAST(RIGHT(' + @NumberColumn + ', 5) AS INT)), 0)
          FROM ' + @TableName + '
          WHERE ' + @NumberColumn + ' LIKE @Pattern',
        N'@Pattern VARCHAR(20), @MaxNum INT OUTPUT',
        @Pattern, @MaxNum OUTPUT;

    SET @Result = @Prefix + '-' + @YearMonth + '-' + RIGHT('00000' + CAST(@MaxNum + 1 AS VARCHAR(5)), 5);
END;
GO

-- SP: Update BinStock after GRN approval (increase stock)
CREATE OR ALTER PROCEDURE sp_ApplyGRN
    @GRNId      INT,
    @ApprovedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Link existing Batch records for lines that match an existing Lot
        UPDATE gl
        SET gl.BatchId = b.BatchId
        FROM GRNLines gl
        CROSS APPLY (
            SELECT TOP 1 BatchId FROM Batches
            WHERE ProductId = gl.ProductId AND LotNumber = gl.LotNumber
        ) b
        WHERE gl.GRNId = @GRNId AND gl.BatchId IS NULL;

        -- Insert new Batches for lines without existing batch
        INSERT INTO Batches (ProductId, SupplierId, LotNumber, MfgDate, ExpiryDate, InitialQty, CreatedBy)
        SELECT gl.ProductId, g.SupplierId, gl.LotNumber, gl.MfgDate, gl.ExpiryDate, gl.Quantity, @ApprovedBy
        FROM GRNLines gl
        JOIN GoodsReceiptNotes g ON g.GRNId = gl.GRNId
        WHERE gl.GRNId = @GRNId AND gl.BatchId IS NULL AND gl.LotNumber IS NOT NULL;

        -- Update BatchId back to lines
        UPDATE gl SET gl.BatchId = b.BatchId
        FROM GRNLines gl
        JOIN Batches b ON b.ProductId = gl.ProductId AND b.LotNumber = gl.LotNumber
        WHERE gl.GRNId = @GRNId AND gl.BatchId IS NULL;

        -- Upsert BinStocks
        MERGE BinStocks AS target
        USING (
            SELECT gl.BinId, gl.BatchId, gl.ProductId, SUM(gl.Quantity) AS Qty
            FROM GRNLines gl WHERE gl.GRNId = @GRNId AND gl.BatchId IS NOT NULL
            GROUP BY gl.BinId, gl.BatchId, gl.ProductId
        ) AS src ON target.BinId = src.BinId AND target.BatchId = src.BatchId
        WHEN MATCHED THEN
            UPDATE SET target.Quantity = target.Quantity + src.Qty, target.UpdatedAt = GETUTCDATE()
        WHEN NOT MATCHED THEN
            INSERT (BinId, BatchId, ProductId, Quantity) VALUES (src.BinId, src.BatchId, src.ProductId, src.Qty);

        -- Log transactions
        INSERT INTO StockTransactions (ProductId, BatchId, BinId, TxnType, DocumentType, DocumentId, DocumentNumber, Quantity, QtyBefore, QtyAfter, CreatedBy)
        SELECT
            gl.ProductId, gl.BatchId, gl.BinId,
            CASE WHEN g.IsReturn = 1 THEN 'GRN_RETURN_OUT' ELSE 'GRN_IN' END,
            'GRN', g.GRNId, g.GRNNumber,
            gl.Quantity,
            bs.Quantity - gl.Quantity,
            bs.Quantity,
            @ApprovedBy
        FROM GRNLines gl
        JOIN GoodsReceiptNotes g ON g.GRNId = gl.GRNId
        JOIN BinStocks bs ON bs.BinId = gl.BinId AND bs.BatchId = gl.BatchId
        WHERE gl.GRNId = @GRNId;

        -- Update GRN status and completion timestamp
        UPDATE GoodsReceiptNotes SET Status = 'APPROVED', UpdatedAt = GETUTCDATE(), CompletedAt = GETUTCDATE() WHERE GRNId = @GRNId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Apply GDN (decrease stock) after DELIVERED
CREATE OR ALTER PROCEDURE sp_ApplyGDN
    @GDNId       INT,
    @DeliveredBy INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Decrease BinStocks and release reservation
        UPDATE bs
        SET bs.Quantity     = bs.Quantity - ISNULL(gl.PickedQty, gl.RequestedQty),
            bs.ReservedQty  = bs.ReservedQty - gl.RequestedQty,
            bs.UpdatedAt    = GETUTCDATE()
        FROM BinStocks bs
        JOIN GDNLines gl ON gl.BinId = bs.BinId AND gl.BatchId = bs.BatchId
        WHERE gl.GDNId = @GDNId;

        -- Log transactions
        INSERT INTO StockTransactions (ProductId, BatchId, BinId, TxnType, DocumentType, DocumentId, DocumentNumber, Quantity, QtyBefore, QtyAfter, CreatedBy)
        SELECT
            gl.ProductId, gl.BatchId, gl.BinId,
            CASE WHEN g.IsReturn = 1 THEN 'GDN_RETURN_IN' ELSE 'GDN_OUT' END,
            'GDN', g.GDNId, g.GDNNumber,
            ISNULL(gl.PickedQty, gl.RequestedQty),
            bs.Quantity + ISNULL(gl.PickedQty, gl.RequestedQty),
            bs.Quantity,
            @DeliveredBy
        FROM GDNLines gl
        JOIN GoodsDispatchNotes g ON g.GDNId = gl.GDNId
        JOIN BinStocks bs ON bs.BinId = gl.BinId AND bs.BatchId = gl.BatchId
        WHERE gl.GDNId = @GDNId;

        UPDATE GoodsDispatchNotes SET Status = 'DELIVERED', DeliveredAt = GETUTCDATE(), DeliveredBy = @DeliveredBy WHERE GDNId = @GDNId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Reserve stock when GDN is APPROVED (before picking)
CREATE OR ALTER PROCEDURE sp_ReserveGDNStock
    @GDNId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE bs
    SET bs.ReservedQty = bs.ReservedQty + gl.RequestedQty,
        bs.UpdatedAt   = GETUTCDATE()
    FROM BinStocks bs
    JOIN GDNLines gl ON gl.BinId = bs.BinId AND gl.BatchId = bs.BatchId
    WHERE gl.GDNId = @GDNId;
END;
GO

-- SP: Apply Transfer Order (move stock from one Bin to another)
CREATE OR ALTER PROCEDURE sp_ApplyTransfer
    @TransferId INT,
    @ApprovedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Decrease from FromBin
        UPDATE bs
        SET bs.Quantity = bs.Quantity - tl.Quantity, bs.UpdatedAt = GETUTCDATE()
        FROM BinStocks bs
        JOIN TransferOrderLines tl ON tl.FromBinId = bs.BinId AND tl.BatchId = bs.BatchId
        WHERE tl.TransferId = @TransferId;

        -- Increase (or insert) at ToBin
        MERGE BinStocks AS target
        USING (
            SELECT tl.ToBinId, tl.BatchId, tl.ProductId, tl.Quantity
            FROM TransferOrderLines tl WHERE tl.TransferId = @TransferId
        ) AS src ON target.BinId = src.ToBinId AND target.BatchId = src.BatchId
        WHEN MATCHED THEN
            UPDATE SET target.Quantity = target.Quantity + src.Quantity, target.UpdatedAt = GETUTCDATE()
        WHEN NOT MATCHED THEN
            INSERT (BinId, BatchId, ProductId, Quantity) VALUES (src.ToBinId, src.BatchId, src.ProductId, src.Quantity);

        -- Log ledger: TRANSFER_OUT + TRANSFER_IN
        INSERT INTO StockTransactions (ProductId, BatchId, BinId, TxnType, DocumentType, DocumentId, DocumentNumber, Quantity, QtyBefore, QtyAfter, CreatedBy)
        SELECT tl.ProductId, tl.BatchId, tl.FromBinId, 'TRANSFER_OUT', 'TRANSFER', t.TransferId, t.TransferNumber,
               tl.Quantity, 0, 0, @ApprovedBy
        FROM TransferOrderLines tl
        JOIN TransferOrders t ON t.TransferId = tl.TransferId
        WHERE tl.TransferId = @TransferId;

        INSERT INTO StockTransactions (ProductId, BatchId, BinId, TxnType, DocumentType, DocumentId, DocumentNumber, Quantity, QtyBefore, QtyAfter, CreatedBy)
        SELECT tl.ProductId, tl.BatchId, tl.ToBinId, 'TRANSFER_IN', 'TRANSFER', t.TransferId, t.TransferNumber,
               tl.Quantity, 0, 0, @ApprovedBy
        FROM TransferOrderLines tl
        JOIN TransferOrders t ON t.TransferId = tl.TransferId
        WHERE tl.TransferId = @TransferId;

        UPDATE TransferOrders SET Status = 'COMPLETED', CompletedAt = GETUTCDATE() WHERE TransferId = @TransferId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Apply Stock Adjustment (from Cycle Count variance)
CREATE OR ALTER PROCEDURE sp_ApplyStockAdjustment
    @AdjustmentId INT,
    @ApprovedBy   INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE bs
        SET bs.Quantity = al.AfterQty, bs.UpdatedAt = GETUTCDATE()
        FROM BinStocks bs
        JOIN StockAdjustmentLines al ON al.BinId = bs.BinId AND al.BatchId = bs.BatchId
        WHERE al.AdjustmentId = @AdjustmentId;

        INSERT INTO StockTransactions (ProductId, BatchId, BinId, TxnType, DocumentType, DocumentId, DocumentNumber, Quantity, QtyBefore, QtyAfter, CreatedBy)
        SELECT
            al.ProductId, al.BatchId, al.BinId,
            CASE WHEN al.DeltaQty >= 0 THEN 'ADJ_IN' ELSE 'ADJ_OUT' END,
            'ADJ', a.AdjustmentId, a.AdjNumber,
            ABS(al.DeltaQty), al.BeforeQty, al.AfterQty, @ApprovedBy
        FROM StockAdjustmentLines al
        JOIN StockAdjustments a ON a.AdjustmentId = al.AdjustmentId
        WHERE al.AdjustmentId = @AdjustmentId;

        UPDATE StockAdjustments SET Status = 'APPROVED', ApprovedAt = GETUTCDATE() WHERE AdjustmentId = @AdjustmentId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ============================================================
-- END OF SCHEMA
-- ============================================================
